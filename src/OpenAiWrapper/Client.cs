using System.Collections.Concurrent;
using System.Collections.ObjectModel;
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
    
    public async Task<OpenAiResponse> GetOpenAiResponse(string text, string threadId, string? pilot = null)
    {
        using OpenAIClient api = new (ApiKey);

        ThreadResponse threadResponse = await api.ThreadsEndpoint.RetrieveThreadAsync(threadId);
        string? user = threadResponse.Metadata.GetValueOrDefault("User");
        if (user == null) throw new MissingFieldException($"Field 'User' is missing in Metadata.");
        string assistantId;

        if (pilot != null)
        {
            assistantId = await GetOrCreateAssistantId(pilot, user, api);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(1));
            assistantId = lastRunResponses.Items.SingleOrDefault()?.AssistantId ?? throw new InvalidOperationException($"No runs were found for the threadId {threadId}.");
        }

        await threadResponse.CreateMessageAsync(new CreateMessageRequest(text));
        
        RunResponse runResponse = await api.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId)).WaitForDone();

        if (runResponse.Status != RunStatus.Completed)
            throw new OperationCanceledException($"Run {runResponse.Id} was ended with the status {Enum.GetName(typeof(RunStatus), runResponse.Status)}.");

        ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(1));
        string? answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
        if (answer == null) throw new InvalidDataException($"No answer was returned from the OpenAI API.");

        return new OpenAiResponse(answer, threadId);
    }
}

public interface IOpenAiClient
{
    OpenAIClient GetNewOpenAiClient { get; }
}