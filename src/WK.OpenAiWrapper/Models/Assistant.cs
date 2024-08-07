﻿using OpenAI;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Constants;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

internal record Assistant(string User, Pilot Pilot) : IEquatable<(string user, string pilot)>
{
    internal CreateAssistantRequest? _createAssistantRequest;

    public CreateAssistantRequest CreateAssistantRequest => _createAssistantRequest ??= GetCreateAssistantRequest();

    public bool Equals((string user, string pilot) keyTuple)
    {
        return string.Equals(User, keyTuple.user, StringComparison.InvariantCultureIgnoreCase) &&
               string.Equals(Pilot.Name, keyTuple.pilot, StringComparison.InvariantCultureIgnoreCase);
    }

    private CreateAssistantRequest GetCreateAssistantRequest()
    {
         return new CreateAssistantRequest(Pilot.Model, UserHelper.GetPilotUserKey(Pilot.Name, User), Pilot.Description, 
             $"{Pilot.Instructions}\r\n{string.Format(Prompts.AiPromptUseName, User.FirstToUpper())}", 
             Pilot.Tools, Pilot.ToolResources, UserHelper.GetDictionaryWithUser(User), 
             responseFormat: Pilot.JsonResponse ? ChatResponseFormat.Json : ChatResponseFormat.Auto);
    }

    public static bool operator ==(Assistant assistant, (string user, string pilot) keyTuple) => assistant.Equals(keyTuple);

    public static bool operator !=(Assistant assistant, (string user, string pilot) keyTuple) => !assistant.Equals(keyTuple);
}