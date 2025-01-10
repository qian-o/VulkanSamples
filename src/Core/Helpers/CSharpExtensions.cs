using System.Reflection;

namespace Graphics.Core.Helpers;

public static class CSharpExtensions
{
    public static void SetPropertyValue<T>(this T obj, string propertyName, object value) where T : class
    {
        if (obj.GetType().GetProperty(propertyName) is not PropertyInfo property)
        {
            return;
        }

        if (property.CanWrite)
        {
            property.SetValue(obj, value);
        }
        else
        {
            obj.GetType().GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(obj, value);
        }
    }
}
