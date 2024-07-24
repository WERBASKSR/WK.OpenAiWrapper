using OpenAI.Assistants;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Models;

namespace WK.OpenAiWrapper.Interfaces;

internal interface IAssistantHandler
{
    ThreadingDictionary<string, string?> AssistantIds { get; }
    ThreadingDictionary<string, Assistant> Assistants { get; }
    HashSet<PilotDescription> PilotDescriptions { get; }
    Assistant GetCreateAssistant(string user, string pilotName);
    CreateAssistantRequest GetCreateAssistantRequest(string user, string pilotName);
    Task<string> GetOrCreateAssistantId(string user, string pilotName);
    Task<AssistantResponse> GetOrCreateAssistantResponse(string user, string pilotName);
}