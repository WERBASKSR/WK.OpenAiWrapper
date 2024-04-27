namespace WK.OpenAiWrapper.Extensions;

internal static class StringExtensions
{
    public static string FirstToUpper(this string input) => string.IsNullOrEmpty(input) ? string.Empty : $"{input.FirstOrDefault().ToString().ToUpper()}{input.Substring(1)}";
}