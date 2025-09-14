using System.Globalization;

namespace ConfigForge.Types;

public class IniFile
{
    private readonly string _path;
    private readonly Dictionary<string, Dictionary<string, string>> _data = new();

    public IniFile(string path)
    {
        _path = path;
        Load();
    }

    public void Load()
    {
        _data.Clear();
        if (!File.Exists(_path)) return;

        string? currentSection = null;
        foreach (var line in File.ReadAllLines(_path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                continue;
            
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                currentSection = trimmed[1..^1].Trim();
                if (!_data.ContainsKey(currentSection))
                    _data[currentSection] = new Dictionary<string, string>();
                
                continue;
            }
            
            var parts = trimmed.Split('=', 2);
            if (parts.Length != 2 || currentSection == null) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
                value = value[1..^1];

            _data[currentSection][key] = value;
        }
    }
    
    public string? GetString(string section, string key, string? defaultValue = null)
        => _data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val) ? val : defaultValue;

    public int GetInt(string section, string key, int defaultValue = 0)
        => int.TryParse(GetString(section, key, defaultValue.ToString()), out var v) ? v : defaultValue;

    public bool GetBool(string section, string key, bool defaultValue = false)
        => bool.TryParse(GetString(section, key, defaultValue.ToString()), out var v) ? v : defaultValue;

    public double GetDouble(string section, string key, double defaultValue = 0.0)
        => double.TryParse(GetString(section, key, defaultValue.ToString(CultureInfo.InvariantCulture)), out var v) ? v : defaultValue;
    
    public void Set(string section, string key, object value)
    {
        if (!_data.ContainsKey(section))
            _data[section] = new Dictionary<string, string>();

        string strValue = value.ToString() ?? "";
        if (strValue.Contains('=') || strValue.StartsWith(' ') || strValue.EndsWith(' '))
            strValue = $"\"{strValue}\"";

        _data[section][key] = strValue;

        Save();
    }

    private void Save()
    {
        var lines = new List<string>();
        foreach (var section in _data)
        {
            lines.Add($"[{section.Key}]");
            lines.AddRange(section.Value.Select(kv => $"{kv.Key}={kv.Value}"));
            lines.Add("");
        }

        File.WriteAllLines(_path, lines);
    }
}