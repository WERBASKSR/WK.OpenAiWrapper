using OpenAI;
using OpenAI.Assistants;
using OpenAI.Threads;

namespace OpenAiWrapper;

internal record Client(AssistantHandler AssistantHandler, string ApiKey) : IOpenAiClient
{
    private readonly ThreadingDictionary<string, string> _assistantIds = new ();

    public OpenAIClient GetNewOpenAiClient => new (ApiKey);

    private async Task<string> GetOrCreateAssistantId(string pilot, string user, OpenAIClient apiClient)
    {
        string pilotUserKey = MiscHelper.GetPilotUserKey(pilot, user);
        if (_assistantIds.ContainsKey(pilotUserKey)) return _assistantIds.GetValue(pilotUserKey);

        ListResponse<AssistantResponse> assistantsResponse = await apiClient.AssistantsEndpoint.ListAssistantsAsync();
        AssistantResponse assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == pilotUserKey) 
                                              ?? await apiClient.AssistantsEndpoint.CreateAssistantAsync(AssistantHandler.GetCreateAssistantRequest(user, pilot));
        _assistantIds.Add(pilotUserKey, assistantResponse.Id);
        return assistantResponse.Id;
    }

    private static async Task<OpenAiResponse> CreateAndSendMessage(string threadId, OpenAIClient apiClient, string assistantId)
    {
        RunResponse runResponse = await apiClient.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId)).WaitForDone();

        if (runResponse.Status != RunStatus.Completed)
            throw new OperationCanceledException($"Run {runResponse.Id} was ended with the status {Enum.GetName(typeof(RunStatus), runResponse.Status)}.");

        ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(1));
        string? answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
        if (answer == null) throw new InvalidDataException($"No answer was returned from the OpenAI API.");

        return new OpenAiResponse(answer, threadId);
    }
    
    public async Task<OpenAiResponse> GetOpenAiResponse(string text, string threadId, string? pilot = null)
    {
        using OpenAIClient apiClient = new (ApiKey);

        ThreadResponse threadResponse = await apiClient.ThreadsEndpoint.RetrieveThreadAsync(threadId);
        string? user = threadResponse.Metadata.GetValueOrDefault("User");
        if (user == null) throw new MissingFieldException($"Field 'User' is missing in Metadata.");
        string assistantId;

        if (pilot != null)
        {
            assistantId = await GetOrCreateAssistantId(pilot, user, apiClient);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(1));
            assistantId = lastRunResponses.Items.SingleOrDefault()?.AssistantId ?? throw new InvalidOperationException($"No runs were found for the threadId {threadId}.");
        }
        
        await threadResponse.CreateMessageAsync(new CreateMessageRequest(text));
        return await CreateAndSendMessage(threadId, apiClient, assistantId);
    }

    public async Task<OpenAiResponse> GetOpenAiResponseWithNewThread(string text, string pilot, string user)
    {
        using OpenAIClient apiClient = new (ApiKey);

        ThreadResponse threadResponse = await apiClient.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new []{ new Message(text)}, MiscHelper.GetDictionaryWithUser(user)));
        string assistantId = await GetOrCreateAssistantId(pilot, user, apiClient);

        return await CreateAndSendMessage(threadResponse.Id, apiClient, assistantId);
    }
}

public interface IOpenAiClient
{
    /// <summary>
    /// Gets a new OpenAI client instance.
    /// </summary>
    /// <returns>
    /// A new OpenAI client instance initialized with the specified API key.
    /// </returns>
    OpenAIClient GetNewOpenAiClient { get; }
    /// <summary>
    /// Gets an OpenAI response within an existing thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="threadId">The ID of the thread in which to retrieve the response.</param>
    /// <param name="pilot">
    /// The optional name of a pilot to influence the response. (Default: null)
    /// </param>
    /// <returns>
    /// An `OpenAiResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `text` or `threadId` is empty or null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// If the thread with the specified ID cannot be found.
    /// </exception>
    Task<OpenAiResponse> GetOpenAiResponse(string text, string threadId, string? pilot = null);
    /// <summary>
    /// Gets an OpenAI response by starting a new thread.
    /// </summary>
    /// <param name="text">The text to send to the OpenAI service.</param>
    /// <param name="pilot">The name of the pilot to influence the response.</param>
    /// <param name="user">The name of the user creating the thread.</param>
    /// <returns>
    /// An `OpenAiResponse` object containing the response from the OpenAI service.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If `text`, `pilot`, or `user` is empty or null.
    /// </exception>
    Task<OpenAiResponse> GetOpenAiResponseWithNewThread(string text, string pilot, string user);
}