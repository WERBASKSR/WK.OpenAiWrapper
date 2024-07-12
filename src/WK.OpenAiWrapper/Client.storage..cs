using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Services;
using OpenAI.VectorStores;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly StorageService StorageService;
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(string[] filePaths, string vectorStoreName, bool waitForDoneStatus = false) 
        => await StorageService.UploadToNewVectorStore(filePaths, vectorStoreName, waitForDoneStatus).ConfigureAwait(false);

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(string[] filePaths, string vectorStoreId, bool waitForDoneStatus = false) 
        => await StorageService.UploadToVectorStore(filePaths, vectorStoreId, waitForDoneStatus).ConfigureAwait(false);

    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToNewVectorStore(Stream fileStream, string fileName, string vectorStoreName, bool waitForDoneStatus = false) =>
        await StorageService.UploadStreamToNewVectorStore(fileStream, fileName, vectorStoreName, waitForDoneStatus).ConfigureAwait(false);

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreName, bool waitForDoneStatus = false) 
        => await StorageService.UploadToNewVectorStore(files, vectorStoreName, waitForDoneStatus).ConfigureAwait(false);

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreId, bool waitForDoneStatus = false) 
        => await StorageService.UploadToVectorStore(files, vectorStoreId, waitForDoneStatus).ConfigureAwait(false);

    public async Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreByName(string fileName, string vectorStoreId) 
        => await StorageService.DeleteFileInVectorStoreByName(fileName, vectorStoreId).ConfigureAwait(false);

    public async Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreById(string fileId, string vectorStoreId) 
        => await StorageService.DeleteFileInVectorStoreById(fileId, vectorStoreId).ConfigureAwait(false);

    public async Task<VectorStoreFileResponse> GetVectorStoreFileStatusAsync(string vectorStoreId, string fileId) 
        => await StorageService.GetVectorStoreFileStatusAsync(vectorStoreId, fileId).ConfigureAwait(false);

    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToVectorStore(Stream fileStream, string fileName, string vectorStoreId, bool waitForDoneStatus = false) =>
        await StorageService.UploadStreamToVectorStore(fileStream, fileName, vectorStoreId, waitForDoneStatus).ConfigureAwait(false);
}