using OpenAI.VectorStores;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models.Responses;

namespace WK.OpenAiWrapper.Interfaces.Clients;

public interface IOpenAiStorageClient
{
    /// <summary>
    /// Uploads a file stream to an existing vector store.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The name of the file being uploaded.</param>
    /// <param name="vectorStoreId">The ID of the vector store to upload the file to.</param>
    /// <param name="waitForDoneStatus">A boolean indicating whether to wait for the operation to complete. (Default: false)</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `fileStream`, `fileName`, or `vectorStoreId` is empty or null.
    /// </exception>
    Task<Result<OpenAiVectorStoreResponse>> UploadStreamToVectorStore(Stream fileStream, string fileName, string vectorStoreId, bool waitForDoneStatus = false);

    /// <summary>
    /// Uploads files to a new vector store.
    /// </summary>
    /// <param name="filePaths">An array of file paths to upload.</param>
    /// <param name="vectorStoreName">The name of the new vector store.</param>
    /// <param name="waitForDoneStatus">A boolean indicating whether to wait for the operation to complete. (Default: false)</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiMultipleFilesVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `filePaths` or `vectorStoreName` is empty or null.
    /// </exception>
    Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(string[] filePaths, string vectorStoreName, bool waitForDoneStatus = false);

    /// <summary>
    /// Uploads files to an existing vector store.
    /// </summary>
    /// <param name="filePaths">An array of file paths to upload.</param>
    /// <param name="vectorStoreId">The ID of the vector store to upload files to.</param>
    /// <param name="waitForDoneStatus">A boolean indicating whether to wait for the operation to complete. (Default: false)</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiMultipleFilesVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `filePaths` or `vectorStoreId` is empty or null.
    /// </exception>
    Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(string[] filePaths, string vectorStoreId, bool waitForDoneStatus = false);

    /// <summary>
    /// Uploads a file stream to a new vector store.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The name of the file being uploaded.</param>
    /// <param name="vectorStoreName">The name of the new vector store.</param>
    /// <param name="waitForDoneStatus">A boolean indicating whether to wait for the operation to complete. (Default: false)</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `fileStream`, `fileName`, or `vectorStoreName` is empty or null.
    /// </exception>
    Task<Result<OpenAiVectorStoreResponse>> UploadStreamToNewVectorStore(Stream fileStream, string fileName, string vectorStoreName, bool waitForDoneStatus = false);


    /// <summary>
    /// Uploads multiple files to a new vector store.
    /// </summary>
    /// <param name="files">A list of tuples containing file names and their corresponding byte arrays to upload.</param>
    /// <param name="vectorStoreName">The name of the new vector store.</param>
    /// <param name="waitForDoneStatus">A boolean indicating whether to wait for the operation to complete. (Default: false)</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiMultipleFilesVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `files` or `vectorStoreName` is empty or null.
    /// </exception>
    Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreName, bool waitForDoneStatus = false);

    /// <summary>
    /// Uploads multiple files to an existing vector store.
    /// </summary>
    /// <param name="files">A list of tuples containing file names and their corresponding byte arrays to upload.</param>
    /// <param name="vectorStoreId">The ID of the vector store to upload files to.</param>
    /// <param name="waitForDoneStatus">A boolean indicating whether to wait for the operation to complete. (Default: false)</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiMultipleFilesVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `files` or `vectorStoreId` is empty or null.
    /// </exception>
    Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreId, bool waitForDoneStatus = false);

    /// <summary>
    /// Deletes a file in a vector store.
    /// </summary>
    /// <param name="fileName">The name of the file to delete.</param>
    /// <param name="vectorStoreId">The ID of the vector store containing the file to delete.</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `fileName` or `vectorStoreId` is empty or null.
    /// </exception>
    Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreByName(string fileName, string vectorStoreId);

    /// <summary>
    /// Deletes a file in a vector store by its ID.
    /// </summary>
    /// <param name="fileId">The ID of the file to delete.</param>
    /// <param name="vectorStoreId">The ID of the vector store containing the file to delete.</param>
    /// <returns>
    /// A `Result` object containing an `OpenAiVectorStoreResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `fileId` or `vectorStoreId` is empty or null.
    /// </exception>
    Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreById(string fileId, string vectorStoreId);

    internal Task<VectorStoreFileResponse> GetVectorStoreFileStatusAsync(string vectorStoreId, string fileId);

}