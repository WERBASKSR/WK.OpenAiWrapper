using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Assistants;
using OpenAiWrapper.Helper;

namespace OpenAiWrapper;

internal class AssistantHandler(IServiceProvider serviceProvider)
{
    private readonly Dictionary<string, string?> _assistantIds = new();
    private readonly ConcurrentBag<Assistant> _assistants = new();

    public CreateAssistantRequest GetCreateAssistantRequest(string user, string pilotName)
    {
        var assistant = _assistants.SingleOrDefault(a => a == (user, pilotName));
        if (assistant != null) return assistant.CreateAssistantRequest;

        var pilot = serviceProvider.GetKeyedService<Pilot>(pilotName) ??
                    throw new NotImplementedException($"{pilotName} is not registered in ServiceCollection");
        assistant = new Assistant(user, pilot);
        _assistants.Add(assistant);

        return assistant.CreateAssistantRequest;
    }

    public async Task<string> GetOrCreateAssistantId(string user, string pilotName, OpenAIClient apiClient)
    {
        return _assistantIds[UserHelper.GetPilotUserKey(pilotName, user)] ??
               (await GetOrCreateAssistantResponse(user, pilotName, apiClient)).Id;
    }

    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string user, string pilotName,
        OpenAIClient apiClient)
    {
        var pilotUserKey = UserHelper.GetPilotUserKey(pilotName, user);
        var assistantId = _assistantIds[pilotUserKey];
        if (assistantId != null) return await apiClient.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);

        ListResponse<AssistantResponse> assistantsResponse = await apiClient.AssistantsEndpoint.ListAssistantsAsync();
        var assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == pilotUserKey)
                                ?? await apiClient.AssistantsEndpoint.CreateAssistantAsync(
                                    GetCreateAssistantRequest(user, pilotName));
        _assistantIds.Add(pilotUserKey, assistantResponse.Id);
        return assistantResponse;
    }

    public async Task<AssistantResponse> GetAssistantResponseAsync(string assistantId)
    {
        using var client = serviceProvider.GetRequiredService<IOpenAiClient>().NewOpenAiClient();
        return await GetAssistantResponseAsync(assistantId, client);
    }

    private static async Task<AssistantResponse> GetAssistantResponseAsync(string assistantId, OpenAIClient apiClient)
    {
        return await apiClient.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);
    }
}