using OpenAI.Assistants;
using WK.OpenAiWrapper.Constants;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

internal record Assistant(string User, Pilot Pilot) : IEquatable<(string user, string pilot)>
{
    private CreateAssistantRequest? _createAssistantRequest;

    public CreateAssistantRequest CreateAssistantRequest => _createAssistantRequest ??= GetCreateAssistantRequest();

    public bool Equals((string user, string pilot) keyTuple)
    {
        return string.Equals(User, keyTuple.user, StringComparison.InvariantCultureIgnoreCase) &&
               string.Equals(Pilot.Name, keyTuple.pilot, StringComparison.InvariantCultureIgnoreCase);
    }

    private CreateAssistantRequest GetCreateAssistantRequest()
    {
        return new CreateAssistantRequest(Pilot.Model, UserHelper.GetPilotUserKey(Pilot.Name, User), description: Pilot.Description,
            $"{Pilot.Instructions}\r\n{string.Format(Prompts.AiPromptUseName, User.FirstToUpper())}", Pilot.Tools,
            metadata: UserHelper.GetDictionaryWithUser(User));
    }

    public static bool operator ==(Assistant assistant, (string user, string pilot) keyTuple)
    {
        return assistant.Equals(keyTuple);
    }

    public static bool operator !=(Assistant assistant, (string user, string pilot) keyTuple)
    {
        return !assistant.Equals(keyTuple);
    }
}