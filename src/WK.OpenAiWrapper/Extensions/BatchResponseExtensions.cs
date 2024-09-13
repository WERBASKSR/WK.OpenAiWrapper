using OpenAI.Batch;

namespace WK.OpenAiWrapper.Extensions;

internal static class BatchResponseExtensions
{
    internal static bool IsDone(this BatchResponse batchResponse)
    {
        switch (batchResponse.Status)
        {
            case BatchStatus.NotStarted:
            case BatchStatus.Validating:
            case BatchStatus.InProgress:
            case BatchStatus.Finalizing:
            case BatchStatus.Cancelling:
                return false;
            case BatchStatus.Cancelled:
            case BatchStatus.Completed:
            case BatchStatus.Expired:
            case BatchStatus.Failed:
                return true;
            default: 
                throw new ArgumentOutOfRangeException();
        }
    }
    internal static bool IsSuccess(this BatchResponse batchResponse) => batchResponse.Status == BatchStatus.Completed;
}