using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;
using OpenAI.VectorStores;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly IStorageService StorageService;
    
    public Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(string[] filePaths, string vectorStoreName, bool waitForDoneStatus = false) 
        => StorageService.UploadToNewVectorStore(filePaths, vectorStoreName, waitForDoneStatus);

    public Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(string[] filePaths, string vectorStoreId, bool waitForDoneStatus = false) 
        => StorageService.UploadToVectorStore(filePaths, vectorStoreId, waitForDoneStatus);

    public Task<Result<OpenAiVectorStoreResponse>> UploadStreamToNewVectorStore(Stream fileStream, string fileName, string vectorStoreName, bool waitForDoneStatus = false)
        => StorageService.UploadStreamToNewVectorStore(fileStream, fileName, vectorStoreName, waitForDoneStatus);

    public Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreName, bool waitForDoneStatus = false) 
        => StorageService.UploadToNewVectorStore(files, vectorStoreName, waitForDoneStatus);

    public Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreId, bool waitForDoneStatus = false) 
        => StorageService.UploadToVectorStore(files, vectorStoreId, waitForDoneStatus);

    public Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreByName(string fileName, string vectorStoreId) 
        => StorageService.DeleteFileInVectorStoreByName(fileName, vectorStoreId);

    public Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreById(string fileId, string vectorStoreId) 
        => StorageService.DeleteFileInVectorStoreById(fileId, vectorStoreId);

    public async Task<VectorStoreFileResponse> GetVectorStoreFileStatusAsync(string vectorStoreId, string fileId) 
        => await StorageService.GetVectorStoreFileStatusAsync(vectorStoreId, fileId).ConfigureAwait(false);

    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToVectorStore(Stream fileStream, string fileName, string vectorStoreId, bool waitForDoneStatus = false) =>
        await StorageService.UploadStreamToVectorStore(fileStream, fileName, vectorStoreId, waitForDoneStatus).ConfigureAwait(false);
}