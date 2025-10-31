using System.Collections;
using System.Reflection;

namespace ConfigForge.Serializers;

public static class YamlDeserializer
{
    public static T Deserialize<T>(string yaml) where T : new()
    {
        var obj = new T();
        var type = typeof(T);
        var lines = yaml.Split('\n');
        PropertyInfo? currentListProperty = null;
        IList? currentList = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;
            
            if (line.TrimStart().StartsWith("-"))
            {
                if (currentListProperty == null || currentList == null)
                    continue;

                var elementType = currentListProperty.PropertyType.GetGenericArguments()[0];
                var rawItemValue = line.TrimStart().TrimStart('-').Trim();
                var parsedItem = ParseValue(rawItemValue, elementType);
                currentList.Add(parsedItem);
                continue;
            }
            
            var parts = line.Split(':', 2);
            if (parts.Length != 2)
                continue;

            var key = parts[0].Trim();
            var rawValue = parts[1].Trim();

            var prop = type.GetProperty(key);
            if (prop == null)
                continue;
            
            if (typeof(IList).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
            {
                var listInstance = (IList)Activator.CreateInstance(prop.PropertyType)!;
                prop.SetValue(obj, listInstance);
                currentListProperty = prop;
                currentList = listInstance;
                continue;
            }
            
            prop.SetValue(obj, ParseValue(rawValue, prop.PropertyType));
            
            currentListProperty = null;
            currentList = null;
        }

        return obj;
    }
    
    private static object ParseValue(string value, Type targetType)
    {
        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
            (value.StartsWith("'") && value.EndsWith("'")))
        {
            value = value[1..^1];
        }

        if (targetType == typeof(string)) return value;
        if (targetType == typeof(int)) return int.TryParse(value, out var i) ? i : 0;
        if (targetType == typeof(double)) return double.TryParse(value, out var d) ? d : 0.0;
        if (targetType == typeof(bool)) return bool.TryParse(value, out var b) && b;

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return Activator.CreateInstance(targetType)!;
        }
    }
}