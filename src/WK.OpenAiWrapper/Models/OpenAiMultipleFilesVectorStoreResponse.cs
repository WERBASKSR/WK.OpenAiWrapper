namespace WK.OpenAiWrapper.Models;

public readonly record struct OpenAiMultipleFilesVectorStoreResponse(string VectorStoreId, List<(string FileName, string FileId)> Files);