using OpenAI.Assistants;
using OpenAI.Threads;

namespace OpenAiWrapper;

internal static class RunResponseExtensions
{
    internal static async Task<RunResponse> WaitForDone(this Task<RunResponse> runResponseTask, AssistantHandler assistantHandler)
    {
        RunResponse runResponse = await runResponseTask;
        return await WaitForDone(runResponse, assistantHandler);
    }

    internal static async Task<RunResponse> WaitForDone(this RunResponse runResponse, AssistantHandler assistantHandler)
    {
        runResponse = await runResponse.WaitForStatusChangeAsync();
        switch (runResponse.Status)
        {
            case RunStatus.RequiresAction:
                AssistantResponse assistantResponse = await assistantHandler.GetAssistantResponse(runResponse.AssistantId);
                IReadOnlyList<ToolOutput> outputs = await assistantResponse.GetToolOutputsAsync(runResponse.RequiredAction.SubmitToolOutputs.ToolCalls);
                runResponse = await runResponse.SubmitToolOutputsAsync(outputs);
                runResponse = await runResponse.WaitForDone(assistantHandler);
                break;
            case RunStatus.InProgress:
            case RunStatus.Queued:
            case RunStatus.Cancelling:
                runResponse = await runResponse.WaitForDone(assistantHandler);
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