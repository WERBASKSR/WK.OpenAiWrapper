using InterfaceFactory;
using OpenAI.Assistants;

namespace WK.OpenAiWrapper.Interfaces.Services;

internal interface IAssistantService : IFactory<IAssistantService>
{
    Task<bool> DeleteAssistantAsync(string assistantId);
    Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId);
    Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest);
    Task<AssistantResponse> ModifyAssistantResponseByIdAsync(string assistantId, CreateAssistantRequest assistantRequest);
    Task<AssistantResponse> ReplaceVectorStoreIdToAssistantByIdAsync(string assistantId, string vectorStoreId);
}