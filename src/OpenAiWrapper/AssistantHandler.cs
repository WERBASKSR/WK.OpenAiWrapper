using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Assistants;

namespace OpenAiWrapper;

internal class AssistantHandler(IServiceProvider serviceProvider)
{
    private readonly ConcurrentBag<Assistant> _assistants = new ();
    private readonly ThreadingDictionary<string, string?> _assistantIds = new ();

    public CreateAssistantRequest GetCreateAssistantRequest(string user, string pilotName)
    {
        Assistant? assistant = _assistants.SingleOrDefault(a => a == (user, pilotName));
        if (assistant != null) return assistant.CreateAssistantRequest;

        Pilot pilot = serviceProvider.GetKeyedService<Pilot>(pilotName) ?? throw new NotImplementedException($"{pilotName} is not registered in ServiceCollection");
        assistant = new Assistant(user, pilot);
        _assistants.Add(assistant);

        return assistant.CreateAssistantRequest;
    }

    public async Task<string> GetOrCreateAssistantId(string user, string pilotName, OpenAIClient apiClient) =>
        _assistantIds.GetValue(MiscHelper.GetPilotUserKey(pilotName, user)) ??
        (await GetOrCreateAssistantResponse(user, pilotName, apiClient)).Id;

    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string user, string pilotName, OpenAIClient apiClient)
    {
        string pilotUserKey = MiscHelper.GetPilotUserKey(pilotName, user);
        string? assistantId = _assistantIds.GetValue(pilotUserKey);
        if (assistantId != null) return await apiClient.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);

        ListResponse<AssistantResponse> assistantsResponse = await apiClient.AssistantsEndpoint.ListAssistantsAsync();
        AssistantResponse assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == pilotUserKey) 
                                              ?? await apiClient.AssistantsEndpoint.CreateAssistantAsync(GetCreateAssistantRequest(user, pilotName));
        _assistantIds.Add(pilotUserKey, assistantResponse.Id);
        return assistantResponse;
    }
    
    public async Task<AssistantResponse> GetAssistantResponse(string assistantId)
    {
        using OpenAIClient client = serviceProvider.GetService<IOpenAiClient>().GetNewOpenAiClient;
        return await GetAssistantResponse(assistantId, client);
    }

    public async Task<AssistantResponse> GetAssistantResponse(string assistantId, OpenAIClient apiClient) => await apiClient.AssistantsEndpoint.RetrieveAssistantAsync(assistantId);
    
}