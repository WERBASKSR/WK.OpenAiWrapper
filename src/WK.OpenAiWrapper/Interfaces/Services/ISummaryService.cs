using OpenAI;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Interfaces.Services;

internal interface ISummaryService
{
    Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10);

    internal Task<Result<OpenAiResponse>> GetConversationSummary(string threadId, OpenAIClient client, int messageCount);

}