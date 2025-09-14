namespace ConfigForge.Serializers;

public static class YamlDeserializer
{
    public static T Deserialize<T>(string yaml) where T : new()
    {
        var obj = new T();
        var type = typeof(T);

        foreach (var line in yaml.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split(':', 2);
            if (parts.Length != 2) 
                continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                value = value[1..^1];
            }

            var prop = type.GetProperty(key);
            if (prop == null) 
                continue;

            try
            {
                object converted;

                if (prop.PropertyType == typeof(string))
                {
                    converted = value;
                }
                else if (prop.PropertyType == typeof(int))
                {
                    converted = int.TryParse(value, out var v) ? v : 0;
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    converted = bool.TryParse(value, out var v) && v;
                }
                else if (prop.PropertyType == typeof(double))
                {
                    converted = double.TryParse(value, out var v) ? v : 0.0;
                }
                else
                {
                    converted = Convert.ChangeType(value, prop.PropertyType);
                }

                prop.SetValue(obj, converted);
            }
            catch
            {
                Console.WriteLine($"Error parsing {key}={value}");
            }
        }

        return obj;
    }
}