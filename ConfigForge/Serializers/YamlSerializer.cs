using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ConfigForge.Serializers;

public static class YamlSerializer
{
    public static string Serialize<T>(T obj)
    {
        var sb = new StringBuilder();
        var type = typeof(T);

        foreach (var prop in type.GetProperties())
        {
            var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null)
                sb.AppendLine($"# {descAttr.Description}");

            var value = prop.GetValue(obj);
            if (value == null)
            {
                sb.AppendLine($"{prop.Name}: null");
                continue;
            }
            
            if (value is IList list && prop.PropertyType.IsGenericType)
            {
                sb.AppendLine($"{prop.Name}:");
                foreach (var item in list)
                {
                    sb.AppendLine($"  - {item}");
                }
            }
            else
            {
                sb.AppendLine($"{prop.Name}: {value}");
            }
        }

        return sb.ToString();
    }
}