using InterfaceFactory;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models.Responses;
using Microsoft.Extensions.Options;
using WK.OpenAiWrapper.Options;
using OpenAI;

namespace WK.OpenAiWrapper.Interfaces.Clients;

public interface IOpenAiClient : IOpenAiStorageClient, IOpenAiSummaryClient, IOpenAiAssumptionClient, IOpenAiAssistantClient, IFactory<IOpenAiClient>
{
    internal IOptions<OpenAiOptions> Options { get; }
    internal IAssistantHandler AssistantHandler { get; }
    internal Task<Result<OpenAiThreadResponse>> GetTextAnswer(string threadId, OpenAIClient client, string assistantId);

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
    /// An `OpenAiThreadResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `text` or `threadId` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// If the thread with the specified ID cannot be found.
    /// </exception>
    Task<Result<OpenAiThreadResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false);

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
    /// An `OpenAiThreadResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `text`, `pilot`, or `user` is empty or null.
    /// </exception>
    Task<Result<OpenAiThreadResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false);
    
    /// <summary>
    ///     Gets an OpenAI response without starting a new thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="systemPrompt">The system prompt to be used by the OpenAI service.</param>
    /// <param name="pilot">The name of the pilot to influence the response.</param>
    /// <returns>
    ///     An `OpenAiChatResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `text`, `systemPrompt`, or `pilot` is empty or null.
    /// </exception>
    Task<Result<OpenAiChatResponse>> GetOpenAiResponseWithoutThread(string text, string systemPrompt, string pilot = null);

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
}