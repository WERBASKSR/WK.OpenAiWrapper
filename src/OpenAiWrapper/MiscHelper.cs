namespace OpenAiWrapper;

internal static class MiscHelper
{
    internal static IReadOnlyDictionary<string, string> GetDictionaryWithUser(string user) => new Dictionary<string, string> { { "User", user } };

    internal static string GetPilotUserKey(string pilot, string user) => $"{pilot}_{user}";
}