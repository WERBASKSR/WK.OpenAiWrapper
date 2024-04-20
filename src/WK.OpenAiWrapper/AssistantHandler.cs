using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;

namespace WK.OpenAiWrapper;

internal class AssistantHandler(IOptions<OpenAiOptions> options)
{
    private readonly ThreadingDictionary<string, string?> _assistantIds = new();
    private readonly ConcurrentBag<Assistant> _assistants = new();

    public CreateAssistantRequest GetCreateAssistantRequest(string user, string pilotName)
    {
        var assistant = _assistants.SingleOrDefault(a => a == (user, pilotName));
        if (assistant != null) return assistant.CreateAssistantRequest;

        
        var pilot = options.Value.Pilots?.SingleOrDefault(p => p.Name == pilotName) ??
                    throw new NotImplementedException($"{pilotName} is not registered in ServiceCollection");
        assistant = new Assistant(user, pilot);
        _assistants.Add(assistant);

        return assistant.CreateAssistantRequest;
    }

    public async Task<string> GetOrCreateAssistantId(string user, string pilotName, OpenAIClient apiClient)
    {
        return _assistantIds.GetValue(UserHelper.GetPilotUserKey(pilotName, user)) ??
               (await GetOrCreateAssistantResponse(user, pilotName, apiClient).ConfigureAwait(false)).Id;
    }

    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string user, string pilotName,
        OpenAIClient apiClient)
    {
        var pilotUserKey = UserHelper.GetPilotUserKey(pilotName, user);
        var assistantId = _assistantIds.GetValue(pilotUserKey);
        if (assistantId != null) return await apiClient.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);

        ListResponse<AssistantResponse> assistantsResponse = await apiClient.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
        var assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == pilotUserKey)
                                ?? await apiClient.AssistantsEndpoint.CreateAssistantAsync(
                                    GetCreateAssistantRequest(user, pilotName)).ConfigureAwait(false);
        _assistantIds.Add(pilotUserKey, assistantResponse.Id);
        return assistantResponse;
    }
}