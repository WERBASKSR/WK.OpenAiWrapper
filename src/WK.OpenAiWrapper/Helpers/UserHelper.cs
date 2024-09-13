namespace WK.OpenAiWrapper.Helpers;

internal static class UserHelper
{
    internal static IReadOnlyDictionary<string, string> GetDictionaryWithUser(string? user)
    {
        return new Dictionary<string, string> { { "User", user ?? string.Empty } };
    }

    internal static string GetPilotUserKey(string pilot, string? user)
    {
        return $"{pilot}_{user??string.Empty}";
    }
}