using InterfaceFactory;
using Microsoft.Extensions.Options;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Interfaces.Clients;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;

namespace WK.OpenAiWrapper;

[IgnoreContainerRegistration]
internal class AssistantHandler(IOptions<OpenAiOptions> options) : IAssistantHandler
{
    public ThreadingDictionary<string, string?> AssistantIds { get; } = new();
    public ThreadingDictionary<string, Assistant> Assistants { get; } = new();
    private readonly Lazy<HashSet<PilotDescription>> _pilotDescriptions = new(() => options.Value.Pilots.Select(p => p.ToPilotDescription()).ToHashSet());
    public HashSet<PilotDescription> PilotDescriptions => _pilotDescriptions.Value;
    
    public Assistant GetCreateAssistant(string? user, string pilotName)
    {
        var pilotUserKey = UserHelper.GetPilotUserKey(pilotName, user);
        var assistant = Assistants.GetValue(pilotUserKey);
        if (assistant != null) return assistant;

        var pilot = options.Value.Pilots?.SingleOrDefault(p => p.Name == pilotName) ?? throw new NotImplementedException($"{pilotName} is not registered in ServiceCollection");
        assistant = new Assistant(user, pilot);
        Assistants.Add(pilotUserKey, assistant);

        return assistant;
    }

    public CreateAssistantRequest GetCreateAssistantRequest(string? user, string pilotName)
        => GetCreateAssistant(user, pilotName).CreateAssistantRequest;

    public async Task<string> GetOrCreateAssistantId(string user, string pilotName) =>
        AssistantIds.GetValue(UserHelper.GetPilotUserKey(pilotName, user)) ??
        (await GetOrCreateAssistantResponse(user, pilotName).ConfigureAwait(false)).Id;

    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string? user, string pilotName)
    {
        var pilotUserKey = UserHelper.GetPilotUserKey(pilotName, user);
        var assistantId = AssistantIds.GetValue(pilotUserKey);
        if (assistantId != null) return await IOpenAiClient.GetRequiredInstance().GetAssistantResponseByIdAsync(assistantId).ConfigureAwait(false);

        var assistantResponse = await IOpenAiClient.GetRequiredInstance().GetOrCreateAssistantResponse(pilotUserKey, GetCreateAssistantRequest(user, pilotName)).ConfigureAwait(false);
        AssistantIds.Add(pilotUserKey, assistantResponse.Id);
        return assistantResponse;
    }
}