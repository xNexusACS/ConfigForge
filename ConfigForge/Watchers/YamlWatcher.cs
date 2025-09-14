using ConfigForge.Serializers;

namespace ConfigForge.Watchers;

public class YamlWatcher<T> where T : new()
{
    private readonly string _path;
    private readonly FileSystemWatcher _watcher;
    
    public T CurrentConfig { get; set; }

    public event Action<T>? OnConfigReloaded;

    public YamlWatcher(string path)
    {
        _path = path;
        LoadConfig();

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(path)!, Path.GetFileName(path));
        _watcher.NotifyFilter = NotifyFilters.LastWrite;
        _watcher.Changed += (_, __) => LoadConfig();
        _watcher.EnableRaisingEvents = true;
    }
    
    private void LoadConfig()
    {
        try
        {
            var yaml = File.ReadAllText(_path);
            CurrentConfig = YamlDeserializer.Deserialize<T>(yaml);
            OnConfigReloaded?.Invoke(CurrentConfig);
        }
        catch
        {
            Console.WriteLine($"Failed to load config from {_path}");
        }
    }
    
    public void Update(Action<T> updateAction)
    {
        updateAction(CurrentConfig);
        
        var yaml = YamlSerializer.Serialize(CurrentConfig);
        File.WriteAllText(_path, yaml);
        
        OnConfigReloaded?.Invoke(CurrentConfig);
    }
}