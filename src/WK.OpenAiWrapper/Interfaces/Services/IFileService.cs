using WK.OpenAiWrapper.Models.Responses;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Interfaces.Services;

internal interface IFileService
{
    Task<Result<OpenAiFilesResponse>> Upload(string[] filePaths, FilePurposeEnum purposeEnum);
    Task<Result<OpenAiFilesResponse>> Upload(Stream fileStream, FilePurposeEnum purposeEnum, string? fileName = null);
}