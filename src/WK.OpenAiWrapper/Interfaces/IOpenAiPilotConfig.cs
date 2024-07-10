using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Interfaces;
public interface IOpenAiPilotConfig
{
    /// <summary>
    /// Retrieves a Pilot by their name.
    /// </summary>
    /// <param name="pilotName">The name of the Pilot to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The task result is a <see cref="Result{T}"/> object that contains the Pilot if found, or null if not found, along with the operation success or failure status.</returns>
    Task<Result<Pilot?>> GetPilot(string pilotName);

    /// <summary>
    /// Adds a new Pilot to the system.
    /// </summary>
    /// <param name="pilot">The Pilot object to add.</param>
    /// <returns>A task representing the asynchronous operation. The task result is a <see cref="Result{T}"/> object that contains the added Pilot and the operation success or failure status.</returns>
    Task<Result<Pilot>> AddPilot(Pilot pilot);

    /// <summary>
    /// Updates an existing Pilot in the system.
    /// </summary>
    /// <param name="pilot">The Pilot object with updated information.</param>
    /// <returns>A task representing the asynchronous operation. The task result is a <see cref="Result{T}"/> object that contains the updated Pilot and the operation success or failure status.</returns>
    Task<Result<Pilot>> UpdatePilot(Pilot pilot);

    /// <summary>
    /// Deletes a Pilot by their name.
    /// </summary>
    /// <param name="pilotName">The name of the Pilot to delete.</param>
    /// <returns>A task representing the asynchronous operation. The task result is a <see cref="Result{T}"/> object that contains the deleted Pilot if the deletion was successful, or null if the Pilot was not found, along with the operation success or failure status.</returns>
    Task<Result<Pilot?>> DeletePilot(string pilotName);
}