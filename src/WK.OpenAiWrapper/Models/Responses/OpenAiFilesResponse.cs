namespace WK.OpenAiWrapper.Models.Responses;

public readonly record struct OpenAiFilesResponse(List<(string FileName, string FileId)> Files);