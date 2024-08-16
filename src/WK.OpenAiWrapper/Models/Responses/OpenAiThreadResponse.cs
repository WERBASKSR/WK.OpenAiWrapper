namespace WK.OpenAiWrapper.Models.Responses;

public readonly record struct OpenAiThreadResponse(string Answer, string ThreadId, string AssistantId);