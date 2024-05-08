using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;

namespace WK.OpenAiWrapper.Interfaces;

public interface IOpenAiClient
{
    /// <summary>
    ///     Gets an OpenAI response within an existing thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="threadId">The ID of the thread in which to retrieve the response.</param>
    /// <param name="pilot">
    ///     The optional name of a pilot to influence the response. (Default: null)
    /// </param>
    /// <returns>
    ///     An `OpenAiResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `text` or `threadId` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the thread with the specified ID cannot be found.
    /// </exception>
    Task<Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null);

    /// <summary>
    ///     Gets an OpenAI response by starting a new thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="pilot">The name of the pilot to influence the response.</param>
    /// <param name="user">The name of the user creating the thread.</param>
    /// <returns>
    ///     An `OpenAiResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `text`, `pilot`, or `user` is empty or null.
    /// </exception>
    Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user);

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
    ///     Gets an OpenAI vision response.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="url">The URL of the image to be analyzed by the OpenAI service.</param>
    /// <returns>
    ///     An `OpenAiResponse` object containing the vision response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `text` or `url` is empty or null.
    /// </exception>
    Task<Result<OpenAiResponse>> GetOpenAiVisionResponse(string text, string url);
    
    /// <summary>
    ///     Gets an OpenAI Pilot Assumption Response.
    /// </summary>
    /// <param name="textToBeAppreciated">The text to be appreciated by the OpenAI service.</param>
    /// <returns>
    ///     A `Result` object containing an `OpenAiPilotAssumptionResponse` from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     If `textToBeAppreciated` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     If the thread with the specified ID cannot be found.
    /// </exception>
    /// <remarks>
    ///     This method creates a new thread with the provided text and available pilots. It then retrieves the text answer from the thread and attempts to deserialize it into a `PilotAssumptionContainer`. If successful, it returns a `Result` object containing an `OpenAiPilotAssumptionResponse`. If an error occurs during deserialization, it returns a `Result` object containing the error message.
    /// </remarks>
    Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeAppreciated);
}