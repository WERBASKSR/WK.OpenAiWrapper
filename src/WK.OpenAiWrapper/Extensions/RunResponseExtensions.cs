﻿using OpenAI.Assistants;
using OpenAI.Threads;
using WK.OpenAiWrapper.Interfaces;

namespace WK.OpenAiWrapper.Extensions;

internal static class RunResponseExtensions
{
    internal static async Task<RunResponse> WaitForDone(this Task<RunResponse> runResponseTask, IAssistantHandler assistantHandler)
    {
        var runResponse = await runResponseTask.ConfigureAwait(false);
        return await WaitForDone(runResponse, assistantHandler).ConfigureAwait(false);
    }

    internal static async Task<RunResponse> WaitForDone(this RunResponse runResponse, IAssistantHandler assistantHandler)
    {
        runResponse = await runResponse.WaitForStatusChangeAsync(timeout: 120).ConfigureAwait(false);
        switch (runResponse.Status)
        {
            case RunStatus.RequiresAction:
                var assistantResponse = await Client.Instance.GetAssistantResponseByIdAsync(runResponse.AssistantId).ConfigureAwait(false);
                IReadOnlyList<ToolOutput> outputs = await assistantResponse.GetToolOutputsAsync(runResponse.RequiredAction.SubmitToolOutputs.ToolCalls).ConfigureAwait(false);
                runResponse = await runResponse.SubmitToolOutputsAsync(outputs).ConfigureAwait(false);
                runResponse = await runResponse.WaitForDone(assistantHandler).ConfigureAwait(false);
                break;
            case RunStatus.InProgress:
            case RunStatus.Queued:
            case RunStatus.Cancelling:
                runResponse = await runResponse.WaitForDone(assistantHandler).ConfigureAwait(false);
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