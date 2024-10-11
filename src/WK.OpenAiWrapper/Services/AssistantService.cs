using OpenAI;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper.Services;

internal class AssistantService : IAssistantService
{
    public async Task<bool> DeleteAssistantAsync(string assistantId)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        return await client.AssistantsEndpoint.DeleteAssistantAsync(assistantId).ConfigureAwait(false);
    }

    public async Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }
    
    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);

        ListResponse<AssistantResponse> assistantsResponse = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);

        AssistantResponse? assistantResponse;
        try
        {
            assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == assistantName)
                                ?? await client.AssistantsEndpoint.CreateAssistantAsync(assistantRequest).ConfigureAwait(false);

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        
        return assistantResponse;
    }

    public async Task<AssistantResponse> ModifyAssistantResponseByIdAsync(string assistantId, CreateAssistantRequest assistantRequest)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        return await client.AssistantsEndpoint.ModifyAssistantAsync(assistantId, assistantRequest);
    }
    
    public async Task<AssistantResponse> ReplaceVectorStoreIdToAssistantByIdAsync(string assistantId, string vectorStoreId)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        AssistantResponse assistant = await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
        CodeInterpreterResources? newCodeInterpreterResources = assistant.ToolResources?.CodeInterpreter;
        FileSearchResources newFileSearchResources = assistant.ToolResources?.FileSearch ?? new FileSearchResources();
        ((List<string>)newFileSearchResources.VectorStoreIds).Add(vectorStoreId);
        ToolResources toolResources = new (newFileSearchResources, newCodeInterpreterResources);
        var assistantRequest = new CreateAssistantRequest(assistant, assistant.Model, assistant.Name, assistant.Description, assistant.Instructions, assistant.Tools, toolResources);
        return await client.AssistantsEndpoint.ModifyAssistantAsync(assistantId, assistantRequest);
    }
}