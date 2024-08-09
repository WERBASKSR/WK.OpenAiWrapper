namespace WK.OpenAiWrapper.Models.Responses;

public readonly record struct OpenAiResponse(string Answer, string ThreadId, string AssistantId);