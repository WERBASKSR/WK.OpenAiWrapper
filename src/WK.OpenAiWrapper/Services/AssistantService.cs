using OpenAI;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Interfaces.Clients;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper.Services;

internal class AssistantService : IAssistantService
{
    public async Task<bool> DeleteAssistantAsync(string assistantId)
    {
        using OpenAIClient client = new (IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
        return await client.AssistantsEndpoint.DeleteAssistantAsync(assistantId).ConfigureAwait(false);
    }

    public async Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId)
    {
        using OpenAIClient client = new (IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }
    
    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest)
    {
        using OpenAIClient client = new (IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);

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
        using OpenAIClient client = new (IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
        return await client.AssistantsEndpoint.ModifyAssistantAsync(assistantId, assistantRequest);
    }
    
    public async Task<AssistantResponse> ReplaceVectorStoreIdToAssistantByIdAsync(string assistantId, string vectorStoreId)
    {
        using OpenAIClient client = new (IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
        var assistant = await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
        var newCodeInterpreterResources = assistant.ToolResources?.CodeInterpreter;
        var newFileSearchResources = new FileSearchResources(vectorStoreId);
        var storeIds = assistant.ToolResources?.FileSearch?.VectorStoreIds;
        if (storeIds != null) ((List<string>)newFileSearchResources.VectorStoreIds).AddRange(storeIds);
        ToolResources toolResources = new (newFileSearchResources, newCodeInterpreterResources);
        var assistantTools = ((List<Tool>)assistant.Tools);
        if (!assistantTools.Contains(Tool.FileSearch)) assistantTools.Add(Tool.FileSearch);
        var assistantRequest = new CreateAssistantRequest(assistant, assistant.Model, assistant.Name, assistant.Description, assistant.Instructions, assistantTools, toolResources);
        return await client.AssistantsEndpoint.ModifyAssistantAsync(assistantId, assistantRequest);
    }
}