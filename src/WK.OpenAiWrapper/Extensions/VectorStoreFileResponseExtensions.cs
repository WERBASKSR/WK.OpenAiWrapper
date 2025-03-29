using OpenAI.Assistants;
using OpenAI.Threads;
using OpenAI.VectorStores;
using WK.OpenAiWrapper.Interfaces.Clients;

namespace WK.OpenAiWrapper.Extensions;

internal static class VectorStoreFileResponseExtensions
{
    internal static async Task<VectorStoreFileResponse> WaitForDone(this Task<VectorStoreFileResponse> vectorStoreFileResponseTask)
    {
        var vectorStoreFileResponse = await vectorStoreFileResponseTask.ConfigureAwait(false);
        return await WaitForDone(vectorStoreFileResponse).ConfigureAwait(false);
    }

    internal static async Task<VectorStoreFileResponse> WaitForDone(this VectorStoreFileResponse vectorStoreFileResponse)
    {
        switch (vectorStoreFileResponse.Status)
        {
            case VectorStoreFileStatus.NotStarted:
            case VectorStoreFileStatus.InProgress:
            case VectorStoreFileStatus.Cancelling:
                vectorStoreFileResponse = await IOpenAiClient.GetRequiredInstance()
                    .GetVectorStoreFileStatusAsync(vectorStoreFileResponse.VectorStoreId, vectorStoreFileResponse.Id)
                    .WaitForDone().ConfigureAwait(false);
                break;
            case VectorStoreFileStatus.Cancelled:
            case VectorStoreFileStatus.Completed:
            case VectorStoreFileStatus.Failed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return vectorStoreFileResponse;
    }
}