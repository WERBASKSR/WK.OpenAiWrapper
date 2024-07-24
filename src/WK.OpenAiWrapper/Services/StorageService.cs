using OpenAI;
using OpenAI.Files;
using OpenAI.VectorStores;
using System.Text.RegularExpressions;
using MoreLinq.Extensions;
using OpenAI.Threads;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper.Services;

internal class StorageService : IStorageService
{
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(string[] filePaths, string vectorStoreName, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName, expiresAfter: 360)).ConfigureAwait(false);
            return await UploadToVectorStore(filePaths, vectorStore.Id, waitForDoneStatus).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(string[] filePaths, string vectorStoreId, bool waitForDoneStatus = false)
    {
        try
        {
            var list = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);

            foreach (var filePath in filePaths)
            {
                FileResponse fileResponse = await client.FilesEndpoint.UploadFileAsync(filePath, FilePurpose.Assistants).ConfigureAwait(false);
                VectorStoreFileResponse vectorStoreFileResponse = await client.VectorStoresEndpoint.CreateVectorStoreFileAsync(vectorStoreId, fileResponse).ConfigureAwait(false);
                if (waitForDoneStatus) await vectorStoreFileResponse.WaitForDone().ConfigureAwait(false);
                list.Add((fileResponse.FileName, fileResponse.Id));
            }

            return new OpenAiMultipleFilesVectorStoreResponse(vectorStoreId, list);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreName, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);

            return await UploadToVectorStore(files, vectorStore.Id, waitForDoneStatus).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreId, bool waitForDoneStatus = false)
    {
        try
        {
            var list = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);

            foreach (var file in files)
            {
                var fileResponseResult = await UploadStreamToVectorStore(new MemoryStream(file.FileBytes), file.FileName, vectorStoreId, waitForDoneStatus).ConfigureAwait(false);
                if (fileResponseResult.IsSuccess) list.Add((file.FileName, fileResponseResult.Value.FileId!));
            }

            return new OpenAiMultipleFilesVectorStoreResponse(vectorStoreId, list);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToNewVectorStore(Stream fileStream, string fileName, string vectorStoreName, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName, expiresAfter: 360)).ConfigureAwait(false);
            return await UploadStreamToVectorStore(fileStream, fileName, vectorStore.Id, waitForDoneStatus).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToVectorStore(Stream fileStream, string fileName, string vectorStoreId, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);
            FileResponse fileResponse = await client.FilesEndpoint.UploadFileAsync(new FileUploadRequest(fileStream, fileName, FilePurpose.Assistants)).ConfigureAwait(false);
            VectorStoreFileResponse vectorStoreFileResponse = await client.VectorStoresEndpoint.CreateVectorStoreFileAsync(vectorStoreId, fileResponse).ConfigureAwait(false);
            await fileStream.DisposeAsync().ConfigureAwait(false);
            if (waitForDoneStatus) await vectorStoreFileResponse.WaitForDone().ConfigureAwait(false);
            return new OpenAiVectorStoreResponse(vectorStoreId, fileResponse.Id);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreByName(string fileName, string vectorStoreId)
    {
        try
        {
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);
            var fileListAll = await client.FilesEndpoint.ListFilesAsync(FilePurpose.Assistants).ConfigureAwait(false);
            var fileIds = fileListAll
                .Where(r => string.Equals(r.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id);
            var vectorStoreFilesAll = await client.VectorStoresEndpoint.ListVectorStoreFilesAsync(vectorStoreId);
            var vectorStoreFile = vectorStoreFilesAll.Items.SingleOrDefault(r => fileIds.Contains(r.Id));
            if (vectorStoreFile != null)
            {
                return await DeleteFileInVectorStoreById(vectorStoreFile.Id, vectorStoreId).ConfigureAwait(false);
            }
            return new OpenAiVectorStoreResponse(vectorStoreId, vectorStoreFile?.Id);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreById(string fileId, string vectorStoreId)
    {
        try
        {
            using OpenAIClient client = new(Client.Instance.Options.Value.ApiKey);
            await client.VectorStoresEndpoint.DeleteVectorStoreFileAsync(vectorStoreId, fileId).ConfigureAwait(false);
            await client.FilesEndpoint.DeleteFileAsync(fileId).ConfigureAwait(false);
            return new OpenAiVectorStoreResponse(vectorStoreId, fileId);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<VectorStoreFileResponse> GetVectorStoreFileStatusAsync(string vectorStoreId, string fileId)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        return await client.VectorStoresEndpoint.GetVectorStoreFileAsync(vectorStoreId, fileId).ConfigureAwait(false);
    }
    
    public async Task<(List<Content> contents, List<(Attachment attachment, string fileName)> attachments, string newVectorStoreId)> GetContentAndAttachmentLists(IEnumerable<string>? attachmentUrls, string? vectorStoreId)
    {
        var contentList = new List<Content>();
        var attachmentList = new List<(Attachment attachment, string fileName)>();

        if (attachmentUrls == null) return (contentList, attachmentList, vectorStoreId);
        
        IEnumerable<string> imageUrls = attachmentUrls.Where(s => Regex.IsMatch(s,
            @"^https?:\/\/[\w.-]+\/[\w.-]*\.(?:gif|jpg|jpeg|png|bmp)$",
            RegexOptions.IgnoreCase));
        imageUrls.ForEach(u => contentList.Add(new Content(ContentType.ImageUrl, u)));
        IEnumerable<string> notImageAttachments = attachmentUrls.Except(imageUrls);
        
        if (!notImageAttachments.Any()) return (contentList, attachmentList, vectorStoreId);
        
        using var httpClient = new HttpClient();
        foreach (string notImageAttachment in notImageAttachments)
        {
            var uri = new Uri(notImageAttachment);
            string fileName = uri.Segments.Last();
            Result<OpenAiVectorStoreResponse> fileResponseResult = vectorStoreId != null
                ? await UploadStreamToVectorStore(
                    await httpClient.GetStreamAsync(uri),
                    fileName,
                    vectorStoreId,
                    true).ConfigureAwait(false)
                : await UploadStreamToNewVectorStore(
                    await httpClient.GetStreamAsync(uri),
                    fileName,
                    $"temp_{Guid.NewGuid()}",
                    true).ConfigureAwait(false);
            if (!fileResponseResult.IsSuccess) throw new Exception(string.Join(';', fileResponseResult.Errors));
            vectorStoreId = fileResponseResult.Value.VectorStoreId;
            attachmentList.Add((new Attachment(fileResponseResult.Value.FileId, Tool.FileSearch), fileName));
        }

        return (contentList, attachmentList, vectorStoreId);
    }
}