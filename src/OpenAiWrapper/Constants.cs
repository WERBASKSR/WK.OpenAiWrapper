namespace OpenAiWrapper;

internal static class Constants
{
    public static IServiceProvider ServiceProvider { get; set; }
    public static Action<string, string> OnThreadExpiredDelegate { get; set; }
}