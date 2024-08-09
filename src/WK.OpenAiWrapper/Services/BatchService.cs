using OpenAI;
using OpenAI.Batch;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces.Services;
using WK.OpenAiWrapper.Models.Responses;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Services;

internal class BatchService(IFileService fileService)
{
    public async Task<OpenAiBatchResponse> CreateBatchAsync(string fileId, string endpoint = "/v1/chat/completions")
    {
        try
        {
            using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
            BatchResponse response = await client.BatchEndpoint.CreateBatchAsync(new CreateBatchRequest(fileId, endpoint)).ConfigureAwait(false);
            return new OpenAiBatchResponse(response.Id, response.IsDone(), response.IsSuccess(), response.BatchErrors.Errors.Select(e => e.Message));
        }
        catch (Exception e)
        {
            return Result<OpenAiBatchResponse>.Error(e.Message);
        }
    }
    
    //public async Task<OpenAiBatchResponse> UploadAndStartBatchAsync(string filePath)
    //{
    //    try
    //    {
    //        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
    //        Result<OpenAiFilesResponse> fileResult = await fileService.Upload([filePath], FilePurposeEnum.Batch);
    //        if (!fileResult.IsSuccess) return string.Join(", ", fileResult.Errors);
    //        BatchResponse response = await client.BatchEndpoint.RetrieveBatchAsync(batchId);

    //        return new OpenAiBatchResponse(response.Id, response.IsDone(), response.IsSuccess(), response.BatchErrors.Errors.Select(e => e.Message));
    //    }
    //    catch (Exception e)
    //    {
    //        return Result<OpenAiBatchResponse>.Error(e.Message);
    //    }
    //}
}