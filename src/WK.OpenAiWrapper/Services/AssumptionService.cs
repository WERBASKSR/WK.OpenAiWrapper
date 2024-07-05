using Newtonsoft.Json;
using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper.Services;

internal class AssumptionService(string assumptionAssistantId)
{
    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        return await GetOpenAiPilotAssumption(textToBeEstimated, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId)
    {
        using OpenAIClient client = new (Client.Instance.Options.Value.ApiKey);
        
        var threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId).ConfigureAwait(false);
        var lastMessageContent = (await threadResponse.ListMessagesAsync(new ListQuery(1)).ConfigureAwait(false)).Items.Single().PrintContent();
        Result<OpenAiResponse> conversationSummary = await Client.Instance.SummaryService.GetConversationSummary(threadId, client, 4);
        string conversationMix = $"Previous Conversation:\n\nSummary: {conversationSummary.Value.Answer}\n\nLast Assistant Message:\n\n{lastMessageContent}";

        return await GetOpenAiPilotAssumption($"{conversationMix}\n\n{textToBeEstimated}", client);
    }
        
    private async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumption(string textToBeEstimated, OpenAIClient client)
    {
        ThreadResponse? threadResponse = null;
        try
        {
            threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
                { new Message($"Prompt: {textToBeEstimated}\r\nAvailable pilots:\r\n{JsonConvert.SerializeObject(Client.Instance.Options.Value.AssistantHandler.PilotDescriptions)}") })).ConfigureAwait(false);
            Result<OpenAiResponse> result = await Client.Instance.GetTextAnswer(threadResponse.Id, client, assumptionAssistantId).ConfigureAwait(false);
            if (!result.IsSuccess) return Result<OpenAiPilotAssumptionResponse>.Error(result.Errors.ToArray());
            return new OpenAiPilotAssumptionResponse(JsonConvert.DeserializeObject<PilotAssumptionContainer>(result.Value.Answer));
        }
        catch (Exception e)
        {
            return Result<OpenAiPilotAssumptionResponse>.Error(e.Message);
        }
        finally
        {
            if (threadResponse?.Id != null) await client.ThreadsEndpoint.DeleteThreadAsync(threadResponse.Id).ConfigureAwait(false);
        }
    }
}