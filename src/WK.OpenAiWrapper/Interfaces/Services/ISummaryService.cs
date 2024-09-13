using OpenAI;
using WK.OpenAiWrapper.Models.Responses;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Interfaces.Services;

internal interface ISummaryService
{
    Task<Result<OpenAiThreadResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10);

    internal Task<Result<OpenAiThreadResponse>> GetConversationSummary(string threadId, OpenAIClient client, int messageCount);

}