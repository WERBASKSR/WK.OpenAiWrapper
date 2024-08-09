namespace WK.OpenAiWrapper.Models.Responses;

public readonly record struct OpenAiBatchResponse(string BatchId, bool IsDone, bool IsSuccess, IEnumerable<string> Errors);