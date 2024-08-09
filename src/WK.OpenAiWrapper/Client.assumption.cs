using WK.OpenAiWrapper.Interfaces.Services;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models.Responses;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly IAssumptionService AssumptionService;
    
    public Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated) 
        => AssumptionService.GetOpenAiPilotAssumptionResponse(textToBeEstimated);

    public Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId)
        => AssumptionService.GetOpenAiPilotAssumptionWithConversationResponse(textToBeEstimated, threadId);
}