using OpenAI.Assistants;
using WK.OpenAiWrapper.Services;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly AssistantService AssistantService;

    public async Task<bool> DeleteAssistantAsync(string assistantId) 
        => await AssistantService.DeleteAssistantAsync(assistantId).ConfigureAwait(false);

    public async Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId) 
        => await AssistantService.GetAssistantResponseByIdAsync(assistantId).ConfigureAwait(false);

    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest)
        => await AssistantService.GetOrCreateAssistantResponse(assistantName, assistantRequest).ConfigureAwait(false);


    public async Task<AssistantResponse> ModifyAssistantResponseByIdAsync(string assistantId, CreateAssistantRequest assistantRequest)
        => await AssistantService.ModifyAssistantResponseByIdAsync(assistantId, assistantRequest).ConfigureAwait(false);

}