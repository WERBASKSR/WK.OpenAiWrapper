namespace WK.OpenAiWrapper.Models.Responses;

public readonly record struct OpenAiMultipleFilesVectorStoreResponse(string VectorStoreId, List<(string FileName, string FileId)> Files);