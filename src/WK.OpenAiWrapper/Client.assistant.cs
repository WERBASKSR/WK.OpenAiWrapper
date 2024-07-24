using OpenAI.Assistants;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly IAssistantService AssistantService;

    public Task<bool> DeleteAssistantAsync(string assistantId) 
        => AssistantService.DeleteAssistantAsync(assistantId);

    public Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId) 
        => AssistantService.GetAssistantResponseByIdAsync(assistantId);

    public Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest)
        => AssistantService.GetOrCreateAssistantResponse(assistantName, assistantRequest);


    public Task<AssistantResponse> ModifyAssistantResponseByIdAsync(string assistantId, CreateAssistantRequest assistantRequest)
        => AssistantService.ModifyAssistantResponseByIdAsync(assistantId, assistantRequest);

}