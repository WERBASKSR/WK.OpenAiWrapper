using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Services;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly AssumptionService AssumptionService;
    
    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated) 
        => await AssumptionService.GetOpenAiPilotAssumptionResponse(textToBeEstimated).ConfigureAwait(false);

    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId)
        => await AssumptionService.GetOpenAiPilotAssumptionWithConversationResponse(textToBeEstimated, threadId).ConfigureAwait(false);
}