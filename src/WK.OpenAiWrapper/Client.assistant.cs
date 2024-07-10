using OpenAI;
using OpenAI.Assistants;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    public async Task<bool> DeleteAssistantAsync(string assistantId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.AssistantsEndpoint.DeleteAssistantAsync(assistantId).ConfigureAwait(false);
    }

    public async Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }
    
    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest)
    {
        try
        {
            using OpenAIClient client = new (Options.Value.ApiKey);

            ListResponse<AssistantResponse> assistantsResponse = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
            var assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == assistantName)
                                    ?? await client.AssistantsEndpoint.CreateAssistantAsync(assistantRequest).ConfigureAwait(false);
            return assistantResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<AssistantResponse> ModifyAssistantResponseByIdAsync(string assistantId, CreateAssistantRequest assistantRequest)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.AssistantsEndpoint.ModifyAssistantAsync(assistantId, assistantRequest);
    }
}