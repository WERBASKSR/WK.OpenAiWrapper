using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("OpenAiWrapper.UnitTests")]


namespace OpenAiWrapper;
internal static class Constants
{
    public static Action<string, string> OnThreadExpiredDelegate { get; set; }

    public const string AiPromptUseName = "In the following conversation, address the conversational partner situationally " +
                                           "and politely as '%'. Use '%' in your responses where it appears natural " +
                                           "and appropriate, to ensure a personal and respectful conversation.";
}