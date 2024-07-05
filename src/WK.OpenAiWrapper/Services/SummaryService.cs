using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Services;

internal class SummaryService(string summaryAssistantId)
{
    public async Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        return await GetConversationSummary(threadId, client, messageCount).ConfigureAwait(false);
    }
        
    internal async Task<Result<OpenAiResponse>> GetConversationSummary(string threadId, OpenAIClient client, int messageCount)
    {
        ThreadResponse? threadResponse = null;
        try
        {
            threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId).ConfigureAwait(false);
            var listMessagesAsync = await threadResponse.ListMessagesAsync(new ListQuery(messageCount)).ConfigureAwait(false);
            var conversation = string.Join("\n\n", listMessagesAsync.Items.Reverse().Select(r => $"{r.Role}: {r.PrintContent()}")); 
            threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
                { new Message(conversation) })).ConfigureAwait(false);
            Result<OpenAiResponse> result = await Client.Instance.GetTextAnswer(threadResponse.Id, client, summaryAssistantId).ConfigureAwait(false);

            return !result.IsSuccess ? Result<OpenAiResponse>.Error(result.Errors.ToArray()) : result;
        }
        catch (Exception e)
        {
            return Result<OpenAiResponse>.Error(e.Message);
        }
        finally
        {
            if (threadResponse?.Id != null) await client.ThreadsEndpoint.DeleteThreadAsync(threadResponse.Id).ConfigureAwait(false);
        }
    }
}