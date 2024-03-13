using OpenAI.Threads;

namespace OpenAiWrapper;

internal static class InternalExtensions
{
    internal static async Task<RunResponse> WaitForDone(this Task<RunResponse> runResponseTask)
    {
        RunResponse runResponse = await runResponseTask;
        return await WaitForDone(runResponse);
    }

    internal static async Task<RunResponse> WaitForDone(this RunResponse runResponse)
    {
        runResponse = await runResponse.WaitForStatusChangeAsync();
        switch (runResponse.Status)
        {
            case RunStatus.RequiresAction:
            case RunStatus.InProgress:
            case RunStatus.Queued:
            case RunStatus.Cancelling:
                await runResponse.WaitForDone();
                break;
            case RunStatus.Cancelled:
            case RunStatus.Failed:
            case RunStatus.Completed:
            case RunStatus.Expired:
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        return runResponse;
    }
}