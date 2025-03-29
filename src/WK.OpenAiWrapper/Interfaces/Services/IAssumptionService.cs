using InterfaceFactory;
using WK.OpenAiWrapper.Models.Responses;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Interfaces.Services;

internal interface IAssumptionService : IFactory<IAssumptionService>
{
    Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated);
    Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId);
}