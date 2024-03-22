using OpenAI;
using OpenAI.Assistants;
using OpenAI.Threads;

namespace OpenAiWrapper;

internal record Client(AssistantHandler AssistantHandler, string ApiKey) : IOpenAiClient
{
    public OpenAIClient GetNewOpenAiClient => new (ApiKey);

    private async Task<Result<OpenAiResponse>> CreateAndSendMessage(string threadId, OpenAIClient apiClient, string assistantId)
    {
        RunResponse runResponse = await apiClient.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId)).WaitForDone(AssistantHandler);

        if (runResponse.Status != RunStatus.Completed) return Result<OpenAiResponse>.Error($"Run {runResponse.Id} was ended with the status {Enum.GetName(typeof(RunStatus), runResponse.Status)}.");

        ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(1));
        string? answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
        if (answer == null) return Result<OpenAiResponse>.Error($"No answer was returned from the OpenAI API.");

        return new OpenAiResponse(answer, threadId);
    }
    
    public async Task<Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null)
    {
        using OpenAIClient apiClient = new (ApiKey);

        ThreadResponse threadResponse = await apiClient.ThreadsEndpoint.RetrieveThreadAsync(threadId);
        string? user = threadResponse.Metadata.GetValueOrDefault("User");
        if (user == null) Result<OpenAiResponse>.Error($"Field 'User' is missing in Metadata.");
        string assistantId;

        if (pilot != null)
        {
            assistantId = await AssistantHandler.GetOrCreateAssistantId(user, pilot, apiClient);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(1));
            string? id = lastRunResponses.Items.SingleOrDefault()?.AssistantId;
            if (id == null) return Result<OpenAiResponse>.Error($"No runs were found for the threadId {threadId}.");
            assistantId = id;
        }
        
        await threadResponse.CreateMessageAsync(new CreateMessageRequest(text));
        return await CreateAndSendMessage(threadId, apiClient, assistantId);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user)
    {
        using OpenAIClient apiClient = new (ApiKey);

        ThreadResponse threadResponse = await apiClient.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new []
            { new Message(text)}, MiscHelper.GetDictionaryWithUser(user)));
        string assistantId = await AssistantHandler.GetOrCreateAssistantId(user, pilot, apiClient);

        return await CreateAndSendMessage(threadResponse.Id, apiClient, assistantId);
    }
}