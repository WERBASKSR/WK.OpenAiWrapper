using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Interfaces;
using Microsoft.Extensions.Options;
using WK.OpenAiWrapper.Options;
using OpenAI.Assistants;

namespace WK.OpenAiWrapper;

internal class Client : IOpenAiClient
{
    private readonly IOptions<OpenAiOptions> _options;
    private readonly AssistantHandler _assistantHandler;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static Client Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Client(IOptions<OpenAiOptions> options)
    {
        _options = options;
        _assistantHandler = new(options);
        Instance = this;
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);

        var threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId).ConfigureAwait(false);
        var user = threadResponse.Metadata.GetValueOrDefault("User");
        if (user == null) Result<OpenAiResponse>.Error("Field 'User' is missing in Metadata.");
        string assistantId;

        if (pilot != null)
        {
            assistantId = await _assistantHandler.GetOrCreateAssistantId(user!, pilot, client).ConfigureAwait(false);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(limit: 1)).ConfigureAwait(false);
            var id = lastRunResponses.Items.SingleOrDefault()?.AssistantId;
            if (id == null) return Result<OpenAiResponse>.Error($"No runs were found for the threadId {threadId}.");
            assistantId = id;
        }

        await threadResponse.CreateMessageAsync(new CreateMessageRequest(text)).ConfigureAwait(false);
        return await CreateAndSendMessage(threadId, client, assistantId).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        var threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
            { new Message(text) }, UserHelper.GetDictionaryWithUser(user))).ConfigureAwait(false);
        var assistantId = await _assistantHandler.GetOrCreateAssistantId(user, pilot, client).ConfigureAwait(false);

        return await CreateAndSendMessage(threadResponse.Id, client, assistantId).ConfigureAwait(false);
    }
    
    internal async Task<AssistantResponse> GetAssistantResponseAsync(string assistantId)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }

    private async Task<Result<OpenAiResponse>> CreateAndSendMessage(string threadId, OpenAIClient apiClient, string assistantId)
    {
        var runResponse = await apiClient.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId))
            .WaitForDone(_assistantHandler).ConfigureAwait(false);

        if (runResponse.Status != RunStatus.Completed)
            return Result<OpenAiResponse>.Error(
                $"Run {runResponse.Id} was ended with the status {Enum.GetName(typeof(RunStatus), runResponse.Status)}.");

        ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(limit: 1)).ConfigureAwait(false);
        var answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
        if (answer == null) return Result<OpenAiResponse>.Error("No answer was returned from the OpenAI API.");

        return new OpenAiResponse(answer, threadId);
    }
}