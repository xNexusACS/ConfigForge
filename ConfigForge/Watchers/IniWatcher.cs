using ConfigForge.Types;

namespace ConfigForge.Watchers;

public class IniWatcher<T> where T : new()
{
    private readonly string _path;
    private readonly FileSystemWatcher _watcher;

    public T CurrentConfig { get; private set; }
    
    public event Action<T>? OnConfigReloaded;
    
    public IniWatcher(string path)
    {
        _path = path;
        
        LoadConfig();
        
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path))
        {
            NotifyFilter = NotifyFilters.LastWrite
        };
        _watcher.Changed += (_, __) => LoadConfig();
        _watcher.EnableRaisingEvents = true;
    }
    
    private void LoadConfig()
    {
        try
        {
            var ini = new IniFile(_path);
            
            var obj = new T();
            var type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                var section = prop.DeclaringType?.Name ?? "Default";
                var key = prop.Name;

                var value = ini.GetString(section, key, null);
                if (value == null) continue;

                try
                {
                    object converted;
                    if (prop.PropertyType == typeof(string))
                        converted = value;
                    else if (prop.PropertyType == typeof(int))
                        converted = int.TryParse(value, out var v) ? v : 0;
                    else if (prop.PropertyType == typeof(bool))
                        converted = bool.TryParse(value, out var v) && v;
                    else if (prop.PropertyType == typeof(double))
                        converted = double.TryParse(value, out var v) ? v : 0.0;
                    else
                        converted = Convert.ChangeType(value, prop.PropertyType);

                    prop.SetValue(obj, converted);
                }
                catch
                {
                    Console.WriteLine($"Failed to convert {value} to {prop.PropertyType.Name}");
                }
            }

            CurrentConfig = obj;
            OnConfigReloaded?.Invoke(CurrentConfig);
        }
        catch
        {
            CurrentConfig = new T();
        }
    }
    
    public void Update(Action<T> updateAction)
    {
        updateAction(CurrentConfig);

        var ini = new IniFile(_path);

        var type = typeof(T);
        foreach (var prop in type.GetProperties())
        {
            var section = prop.DeclaringType?.Name ?? "Default";
            var value = prop.GetValue(CurrentConfig)?.ToString() ?? "";
            ini.Set(section, prop.Name, value);
        }

        OnConfigReloaded?.Invoke(CurrentConfig);
    }
}