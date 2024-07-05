using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;

namespace WK.OpenAiWrapper.Interfaces;
public interface IOpenAiAssumptionClient
{ 
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
}