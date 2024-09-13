namespace WK.OpenAiWrapper.Models.Responses;

public readonly record struct OpenAiVectorStoreResponse(string VectorStoreId, string? FileId = null);