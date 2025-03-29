using OpenAI;
using OpenAI.Files;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models.Responses;
using WK.OpenAiWrapper.Interfaces.Services;
using WK.OpenAiWrapper.Interfaces.Clients;

namespace WK.OpenAiWrapper.Services;

internal class FileService : IFileService
{
    private static readonly List<string> AllowedUploadFilePurposes =
    [
        FilePurpose.Assistants,
        FilePurpose.Batch,
        FilePurpose.FineTune,
        FilePurpose.Vision
    ];
    
    public async Task<Result<OpenAiFilesResponse>> Upload(string[] filePaths, FilePurposeEnum purposeEnum)
    {
        try
        {
            var purpose = purposeEnum.ConvertToString();
            if (!AllowedUploadFilePurposes.Contains(purpose)) return Result<OpenAiFilesResponse>.Error($"File purpose type '{purpose}' is not allowed to upload.");
            
            var files = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
            foreach (var filePath in filePaths)
            {
                var fileResponse = await client.FilesEndpoint.UploadFileAsync(filePath, purpose);
                files.Add((fileResponse.FileName, fileResponse.Id));
            }

            return new OpenAiFilesResponse(files);
        }
        catch (Exception e)
        {
            return Result<OpenAiFilesResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiFilesResponse>> Upload(Stream fileStream, FilePurposeEnum purposeEnum, string? fileName = null)
    {
        try
        {
            var purpose = purposeEnum.ConvertToString();
            if (!AllowedUploadFilePurposes.Contains(purpose)) return Result<OpenAiFilesResponse>.Error($"File purpose type '{purpose}' is not allowed to upload.");
            fileName ??= Guid.NewGuid().ToString();
            using OpenAIClient client = new(IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
            var fileResponse = await client.FilesEndpoint.UploadFileAsync(new FileUploadRequest(fileStream, fileName, purpose));
            return new OpenAiFilesResponse([(fileResponse.FileName, fileResponse.Id)]);
        }
        catch (Exception e)
        {
            return Result<OpenAiFilesResponse>.Error(e.Message);
        }
    }
}