using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace WK.OpenAiWrapper.Extensions;

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> EnumToStringCache = new ();
    private static readonly ConcurrentDictionary<string, Enum> StringToEnumCache = new ();

    public static string ConvertToString<T>(this T enumValue) where T : Enum =>
        EnumToStringCache.GetOrAdd(enumValue, key => 
            typeof(T).GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<EnumMemberAttribute>()?.Value 
            ?? throw new ArgumentNullException("attr.Value"));

    public static T ConvertToEnum<T>(this string value) where T : Enum =>
        (T)StringToEnumCache.GetOrAdd(value, key =>
        {
            var type = typeof(T);
            foreach (var field in type.GetFields())
            {
                if (field.GetCustomAttribute<EnumMemberAttribute>()?.Value == value) return (T)field.GetValue(null);
            }
            throw new ArgumentException($"No matching enum value found for '{value}' in {type}.");
        });
}