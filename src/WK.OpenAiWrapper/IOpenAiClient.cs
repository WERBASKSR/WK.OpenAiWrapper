using WK.OpenAiWrapper.Result;
using OpenAI;

namespace WK.OpenAiWrapper;

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
}