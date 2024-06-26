using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;
using OpenAI.Assistants;
using OpenAI;
using OpenAI.VectorStores;
namespace WK.OpenAiWrapper.Interfaces;
public interface IOpenAiClient
{ 
    /// <summary>
    /// Gets an OpenAI response within an existing thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="threadId">The ID of the thread in which to retrieve the response.</param>
    /// <param name="pilot">
    /// The optional name of a pilot to influence the response. (Default: null)
    /// </param>
    /// <param name="attachmentUrls">
    /// The optional URLs of attachments to be processed by the OpenAI service. (Default: null)
    /// </param>
    /// <param name="deleteFilesAfterUse">
    /// A boolean indicating whether to delete the files after use. (Default: false)
    /// </param>
    /// <returns>
    /// An `OpenAiResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `text` or `threadId` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// If the thread with the specified ID cannot be found.
    /// </exception>
    Task<Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false);

    /// <summary>
    /// Gets an OpenAI response by starting a new thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="pilot">The name of the pilot to influence the response.</param>
    /// <param name="user">The name of the user creating the thread.</param>
    /// <param name="attachmentUrls">
    /// The optional URLs of attachments to be processed by the OpenAI service. (Default: null)
    /// </param>
    /// <param name="deleteFilesAfterUse">
    /// A boolean indicating whether to delete the files after use. (Default: false)
    /// </param>
    /// <returns>
    /// An `OpenAiResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `text`, `pilot`, or `user` is empty or null.
    /// </exception>
    Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false);

    /// <summary>
    ///     Gets an OpenAI image response.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <returns>
    ///     An `OpenAiImageResponse` object containing the image response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `text` is empty or null.
    /// </exception>
    Task<Result<OpenAiImageResponse>> GetOpenAiImageResponse(string text);

    /// <summary>
    ///     Gets an OpenAI audio response.
    /// </summary>
    /// <param name="audioFilePath">The audio filepath to send to the OpenAI service.</param>
    /// <returns>
    ///     An `OpenAiAudioResponse` object containing the audio response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `audio` is null.
    /// </exception>
    Task<Result<OpenAiAudioResponse>> GetOpenAiAudioResponse(string audioFilePath);
    
    /// <summary>
    ///     Gets an OpenAI speech response.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <returns>
    ///     An `OpenAiSpeechResponse` object containing the speech response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `text` is empty or null.
    /// </exception>
    Task<Result<OpenAiSpeechResponse>> GetOpenAiSpeechResponse(string text);
    
    /// <summary>
    ///     Gets an OpenAI Pilot Assumption Response.
    /// </summary>
    /// <param name="textToBeEstimated">The text to be estimated by the OpenAI service.</param>
    /// <returns>
    ///     A `Result` object containing an `OpenAiPilotAssumptionResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `textToBeEstimated` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the thread with the specified ID cannot be found.
    /// </exception>
    /// <remarks>
    ///     This method creates a new thread with the provided text and available pilots.
    ///     It then retrieves the text answer from the thread and attempts to deserialize it into a `PilotAssumptionContainer`.
    ///     If successful, it returns a `Result` object containing an `OpenAiPilotAssumptionResponse`.
    ///     If an error occurs during deserialization, it returns a `Result` object containing the error message.
    /// </remarks>
    Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated);
    
    /// <summary>
    ///     Gets an OpenAI Pilot Assumption Response within an existing thread.
    /// </summary>
    /// <param name="textToBeEstimated">The text to be estimated by the OpenAI service.</param>
    /// <param name="threadId">The ThreadId of the conversation to be used for the estimation.</param>
    /// <returns>
    ///     A `Result` object containing an `OpenAiPilotAssumptionResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `textToBeEstimated` or `threadId` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the thread with the specified ID cannot be found.
    /// </exception>
    /// <remarks>
    ///     This method estimates which pilot is most suitable for the given prompt, taking into account the previous conversation.
    ///     It retrieves the text answer from the thread and attempts to deserialize it into a `PilotAssumptionContainer`.
    ///     If successful, it returns a `Result` object containing an `OpenAiPilotAssumptionResponse`.
    ///     If an error occurs during deserialization, it returns a `Result` object containing the error message.
    /// </remarks>
    Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId);
    
    /// <summary>
    ///     Gets a summary of the current conversation between the OpenAI service and the specified user.
    /// </summary>
    /// <param name="user">The name of the user whose conversation with the OpenAI service is to be summarized.</param>
    /// <param name="messageCount">The number of recent messages to include in the summary. (Default: 10)</param>
    /// <returns>
    ///     A `Result` object containing an `OpenAiResponse` from the OpenAI service, which includes the summary of the conversation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `user` is empty or null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If `messageCount` is less than 1.
    /// </exception>
    /// <remarks>
    ///     This method retrieves the specified number of recent messages from the conversation between the specified user
    ///     and the OpenAI service, and returns a summary of those messages.
    /// </remarks>
    Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string user, int messageCount = 10);
    
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

    internal Task<bool> DeleteAssistantAsync(string assistantId);
    internal Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId);
    internal Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest);
    internal Task<VectorStoreFileResponse> GetVectorStoreFileStatusAsync(string vectorStoreId, string fileId);
}