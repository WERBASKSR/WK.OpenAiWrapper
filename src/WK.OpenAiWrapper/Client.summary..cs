using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Services;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly SummaryService SummaryService;
        
    public async Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10) 
        => await SummaryService.GetConversationSummaryResponse(threadId, messageCount);
}