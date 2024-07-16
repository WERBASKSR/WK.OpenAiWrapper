using OpenAI.Assistants;

namespace WK.OpenAiWrapper.Interfaces.Clients;
public interface IOpenAiAssistantClient
{
    internal Task<bool> DeleteAssistantAsync(string assistantId);
    internal Task<AssistantResponse> ModifyAssistantResponseByIdAsync(string assistantId, CreateAssistantRequest assistantRequest);
    internal Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId);
    internal Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest);
}