using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;

namespace WK.OpenAiWrapper.Interfaces.Clients;
public interface IOpenAiSummaryClient
{
    /// <summary>
    ///     Gets a summary of the current conversation between the OpenAI service and the specified threadId.
    /// </summary>
    /// <param name="threadId">The threadId of the conversation with the OpenAI service is to be summarized.</param>
    /// <param name="messageCount">The number of recent messages to include in the summary. (Default: 10)</param>
    /// <returns>
    ///     A `Result` object containing an `OpenAiResponse` from the OpenAI service, which includes the summary of the conversation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `threadId` is empty or null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If `messageCount` is less than 1.
    /// </exception>
    /// <remarks>
    ///     This method retrieves the specified number of recent messages from the conversation between the specified threadId
    ///     and the OpenAI service, and returns a summary of those messages.
    /// </remarks>
    Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10);
}