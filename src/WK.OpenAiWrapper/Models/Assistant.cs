using OpenAI;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Constants;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

internal record Assistant(string? User, Pilot Pilot) : IEquatable<(string user, string pilot)>
{
    internal CreateAssistantRequest? _createAssistantRequest;

    public CreateAssistantRequest CreateAssistantRequest => _createAssistantRequest ??= GetCreateAssistantRequest();

    public bool Equals((string? user, string pilot) keyTuple)
    {
        return string.Equals(User, keyTuple.user, StringComparison.InvariantCultureIgnoreCase) &&
               string.Equals(Pilot.Name, keyTuple.pilot, StringComparison.InvariantCultureIgnoreCase);
    }

    private CreateAssistantRequest GetCreateAssistantRequest()
    {
        string userPrompt = User != null ? string.Format(Prompts.AiPromptUseName, User.FirstToUpper()) : string.Empty;
        return new CreateAssistantRequest(Pilot.Model, UserHelper.GetPilotUserKey(Pilot.Name, User), Pilot.Description, 
             $"{Pilot.Instructions}\r\n{userPrompt}", 
             Pilot.Tools, Pilot.ToolResources, UserHelper.GetDictionaryWithUser(User), 
             responseFormat: Pilot.JsonResponse ? ChatResponseFormat.Json : ChatResponseFormat.Text);
    }

    public static bool operator ==(Assistant assistant, (string user, string pilot) keyTuple) => assistant.Equals(keyTuple);

    public static bool operator !=(Assistant assistant, (string user, string pilot) keyTuple) => !assistant.Equals(keyTuple);
}