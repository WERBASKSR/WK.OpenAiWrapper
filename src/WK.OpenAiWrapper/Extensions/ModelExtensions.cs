using OpenAI;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Constants;

namespace WK.OpenAiWrapper.Extensions;

internal static class OpenAiClientExtensions
{
    public static async Task<AssistantResponse> GetSummaryAssistant(this OpenAIClient client)
    {
        string summaryAssistantName = "SummaryAssistant";
        ListResponse<AssistantResponse> assistantResponses = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
        AssistantResponse? assistantResponse = assistantResponses.Items.SingleOrDefault(a => a.Name == summaryAssistantName) ?? 
                                               await client.AssistantsEndpoint.CreateAssistantAsync(new CreateAssistantRequest(
                                                       "gpt-4o",
                                                       summaryAssistantName, 
                                                       "", 
                                                       Prompts.AiConversationSummaryPrompt))
                                                   .ConfigureAwait(false);
        return assistantResponse;
    }
    
    public static async Task<AssistantResponse> GetAssumptionAssistant(this OpenAIClient client)
    {
        string assumptionAssistantName = "AssumptionAssistant";
        ListResponse<AssistantResponse> assistantResponses = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
        AssistantResponse? assistantResponse = assistantResponses.Items.SingleOrDefault(a => a.Name == assumptionAssistantName) ??
                                               await client.AssistantsEndpoint.CreateAssistantAsync(new CreateAssistantRequest(
                                                       "gpt-4o",
                                                       assumptionAssistantName,
                                                       "",
                                                       Prompts.AiAssumptionPrompt, 
                                                       responseFormat: ChatResponseFormat.Json))
                                                   .ConfigureAwait(false);
        return assistantResponse;
    }
}