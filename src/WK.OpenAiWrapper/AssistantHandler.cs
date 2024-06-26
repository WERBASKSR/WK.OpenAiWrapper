using Microsoft.Extensions.Options;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;

namespace WK.OpenAiWrapper;

internal class AssistantHandler(IOptions<OpenAiOptions> options)
{
    internal readonly ThreadingDictionary<string, string?> AssistantIds = new();
    internal readonly ThreadingDictionary<string, Assistant> Assistants = new();
    private readonly Lazy<HashSet<PilotDescription>> _pilotDescriptions = new(() => options.Value.Pilots.Select(p => p.ToPilotDescription()).ToHashSet());
    public HashSet<PilotDescription> PilotDescriptions => _pilotDescriptions.Value;
    
    public CreateAssistantRequest GetCreateAssistantRequest(string user, string pilotName)
    {
        string pilotUserKey = UserHelper.GetPilotUserKey(pilotName, user);
        var assistant = Assistants.GetValue(pilotUserKey);
        if (assistant != null) return assistant.CreateAssistantRequest;

        var pilot = options.Value.Pilots?.SingleOrDefault(p => p.Name == pilotName) ?? throw new NotImplementedException($"{pilotName} is not registered in ServiceCollection");
        assistant = new Assistant(user, pilot);
        Assistants.Add(pilotUserKey, assistant);

        return assistant.CreateAssistantRequest;
    }

    public async Task<string> GetOrCreateAssistantId(string user, string pilotName, IOpenAiClient client) =>
        AssistantIds.GetValue(UserHelper.GetPilotUserKey(pilotName, user)) ??
        (await GetOrCreateAssistantResponse(user, pilotName, client).ConfigureAwait(false)).Id;

    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string user, string pilotName, IOpenAiClient client)
    {
        var pilotUserKey = UserHelper.GetPilotUserKey(pilotName, user);
        var assistantId = AssistantIds.GetValue(pilotUserKey);
        if (assistantId != null) return await client.GetAssistantResponseByIdAsync(assistantId).ConfigureAwait(false);

        AssistantResponse assistantResponse = await client.GetOrCreateAssistantResponse(pilotUserKey, GetCreateAssistantRequest(user, pilotName)).ConfigureAwait(false);
        AssistantIds.Add(pilotUserKey, assistantResponse.Id);
        return assistantResponse;
    }
}