using OpenAI.Assistants;

namespace OpenAiWrapper;

internal record Assistant(string User, Pilot Pilot) : IEquatable<(string user, string pilot)>
{
    private CreateAssistantRequest? _createAssistantRequest;

    public CreateAssistantRequest CreateAssistantRequest => _createAssistantRequest ??= GetCreateAssistantRequest();

    private CreateAssistantRequest GetCreateAssistantRequest()
    {
        return new CreateAssistantRequest(Pilot.Model, MiscHelper.GetPilotUserKey(Pilot.Name, User), null, 
            $"{Pilot.Instructions}\r\n{string.Format(Prompts.AiPromptUseName, User)}", Pilot.Tools, null, MiscHelper.GetDictionaryWithUser(User));
    }

    public static bool operator== (Assistant assistant, (string user, string pilot) keyTuple)
    {
        return assistant.Equals(keyTuple);
    }

    public static bool operator !=(Assistant assistant, (string user, string pilot) keyTuple)
    {
        return !assistant.Equals(keyTuple);
    }

    public bool Equals((string user, string pilot) keyTuple) =>
        string.Equals(User, keyTuple.user, StringComparison.InvariantCultureIgnoreCase) &&
        string.Equals(Pilot.Name, keyTuple.pilot, StringComparison.InvariantCultureIgnoreCase);
}