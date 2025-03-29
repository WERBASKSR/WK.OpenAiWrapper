using InterfaceFactory;
using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Interfaces.Clients;
using WK.OpenAiWrapper.Interfaces.Services;
using WK.OpenAiWrapper.Models.Responses;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Services;

[IgnoreContainerRegistration]
internal class SummaryService(string summaryAssistantId) : ISummaryService
{
    public async Task<Result<OpenAiThreadResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10)
    {
        using OpenAIClient client = new (IOpenAiClient.GetRequiredInstance().Options.Value.ApiKey);
        return await GetConversationSummary(threadId, client, messageCount).ConfigureAwait(false);
    }
        
    public async Task<Result<OpenAiThreadResponse>> GetConversationSummary(string threadId, OpenAIClient client, int messageCount)
    {
        ThreadResponse? threadResponse = null;
        try
        {
            threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId).ConfigureAwait(false);
            var listMessagesAsync = await threadResponse.ListMessagesAsync(new ListQuery(messageCount)).ConfigureAwait(false);
            var conversation = string.Join("\n\n", listMessagesAsync.Items.Reverse().Select(r => $"{r.Role}: {r.PrintContent()}")); 
            threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
                { new Message(conversation) })).ConfigureAwait(false);
            Result<OpenAiThreadResponse> result = await IOpenAiClient.GetRequiredInstance().GetTextAnswer(threadResponse.Id, client, summaryAssistantId).ConfigureAwait(false);

            return !result.IsSuccess ? Result<OpenAiThreadResponse>.Error(result.Errors.ToArray()) : result;
        }
        catch (Exception e)
        {
            return Result<OpenAiThreadResponse>.Error(e.Message);
        }
        finally
        {
            if (threadResponse?.Id != null) await client.ThreadsEndpoint.DeleteThreadAsync(threadResponse.Id).ConfigureAwait(false);
        }
    }
}