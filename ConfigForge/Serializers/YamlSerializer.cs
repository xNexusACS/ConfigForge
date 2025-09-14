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
            sb.AppendLine($"{prop.Name}: {value}");
        }

        return sb.ToString();
    }
}