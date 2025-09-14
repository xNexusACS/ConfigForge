using System.Diagnostics;
using ConfigForge.Serializers;
using ConfigForge.Types;
using ConfigForge.Watchers;

namespace ConfigForge;

public class ConfigManager
{
    private readonly string _configPath;
    
    public ConfigManager()
    {
        var execPath = Process.GetCurrentProcess().MainModule?.FileName;
        var basePath = execPath != null ? Path.GetDirectoryName(execPath)! : AppContext.BaseDirectory;

        _configPath = Path.Combine(basePath, "configs");
        Directory.CreateDirectory(_configPath);
    }
    
    public YamlWatcher<T> InitYamlConfig<T>(string fileName) where T : new()
    {
        var path = Path.Combine(_configPath, fileName);

        if (File.Exists(path)) 
            return new YamlWatcher<T>(path);
        
        var obj = new T();
        var yaml = YamlSerializer.Serialize(obj);
        File.WriteAllText(path, yaml);

        return new YamlWatcher<T>(path);
    }
    
    public IniWatcher<T> InitIniConfig<T>(string fileName) where T : new()
    {
        var path = Path.Combine(_configPath, fileName);

        if (File.Exists(path)) 
            return new IniWatcher<T>(path);
        
        var obj = new T();
        var ini = new IniFile(path);

        var type = typeof(T);
        foreach (var prop in type.GetProperties())
        {
            var section = prop.DeclaringType?.Name ?? "Default";
            var value = prop.GetValue(obj)?.ToString() ?? "";
            ini.Set(section, prop.Name, value);
        }

        return new IniWatcher<T>(path);
    }
}