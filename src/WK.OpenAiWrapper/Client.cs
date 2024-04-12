using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Helper;

namespace WK.OpenAiWrapper;

internal class Client : IOpenAiClient
{
    private readonly string _apiKey;
    private readonly AssistantHandler _assistantHandler;

    public Client(AssistantHandler assistantHandler, string apiKey)
    {
        _assistantHandler = assistantHandler;
        _apiKey = apiKey;
    }

    public OpenAIClient NewOpenAiClient() => new(_apiKey);

    public async Task<Result.Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null)
    {
        using OpenAIClient apiClient = new(_apiKey);

        var threadResponse = await apiClient.ThreadsEndpoint.RetrieveThreadAsync(threadId);
        var user = threadResponse.Metadata.GetValueOrDefault("User");
        if (user == null) Result<OpenAiResponse>.Error("Field 'User' is missing in Metadata.");
        string assistantId;

        if (pilot != null)
        {
            assistantId = await _assistantHandler.GetOrCreateAssistantId(user!, pilot, apiClient);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(limit: 1));
            var id = lastRunResponses.Items.SingleOrDefault()?.AssistantId;
            if (id == null) return Result<OpenAiResponse>.Error($"No runs were found for the threadId {threadId}.");
            assistantId = id;
        }

        await threadResponse.CreateMessageAsync(new CreateMessageRequest(text));
        return await CreateAndSendMessage(threadId, apiClient, assistantId);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user)
    {
        using OpenAIClient apiClient = new(_apiKey);

        var threadResponse = await apiClient.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
            { new Message(text) }, UserHelper.GetDictionaryWithUser(user)));
        var assistantId = await _assistantHandler.GetOrCreateAssistantId(user, pilot, apiClient);

        return await CreateAndSendMessage(threadResponse.Id, apiClient, assistantId);
    }

    private async Task<Result<OpenAiResponse>> CreateAndSendMessage(string threadId, OpenAIClient apiClient,
        string assistantId)
    {
        var runResponse = await apiClient.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId))
            .WaitForDone(_assistantHandler);

        if (runResponse.Status != RunStatus.Completed)
            return Result<OpenAiResponse>.Error(
                $"Run {runResponse.Id} was ended with the status {Enum.GetName(typeof(RunStatus), runResponse.Status)}.");

        ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(limit: 1));
        var answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
        if (answer == null) return Result<OpenAiResponse>.Error("No answer was returned from the OpenAI API.");

        return new OpenAiResponse(answer, threadId);
    }
}