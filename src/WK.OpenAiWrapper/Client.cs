using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WK.OpenAiWrapper.Options;
using OpenAI.Assistants;
using OpenAI.Audio;
using OpenAI.Images;
using OpenAI.Models;
using OpenAI.Chat;
using WK.OpenAiWrapper.Constants;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using Message = OpenAI.Threads.Message;

namespace WK.OpenAiWrapper;

internal class Client : IOpenAiClient
{
    private readonly string AssumptionAssistantId;
    private readonly string VisionAssistantId;
    internal readonly IOptions<OpenAiOptions> _options;
    private readonly AssistantHandler _assistantHandler;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static Client Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Client(IOptions<OpenAiOptions> options)
    {
        _options = options;
        _assistantHandler = new(options);

        AssumptionAssistantId = Task.Run(GetAssumptionAssistantId).GetAwaiter().GetResult() ??
                                throw new ArgumentNullException(
                                    $"{nameof(AssumptionAssistantId)}: The AssumptionAssistant could not be retrieved or created. ");
        
        VisionAssistantId = Task.Run(GetVisionSummaryAssistantId).GetAwaiter().GetResult() ??
                                throw new ArgumentNullException(
                                    $"{nameof(VisionAssistantId)}: The AssumptionAssistant could not be retrieved or created. ");
        Instance = this;
    }

    
    private async Task<string> GetConversationSummaryAssistantId()
    {
        string summaryAssistantName = "SummaryAssistant";
        using OpenAIClient client = new (_options.Value.ApiKey);
        ListResponse<AssistantResponse> assistantResponses = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
        AssistantResponse? assistantResponse = assistantResponses.Items.SingleOrDefault(a => a.Name == summaryAssistantName) ?? 
            await client.AssistantsEndpoint.CreateAssistantAsync(new CreateAssistantRequest(
                "gpt-4o",
                summaryAssistantName, 
                "", 
                Prompts.AiConversationSummaryPrompt))
            .ConfigureAwait(false);
        return assistantResponse.Id;
    }
    
    private async Task<string> GetVisionSummaryAssistantId()
    {
        string summaryAssistantName = "VisionAssistant";
        using OpenAIClient client = new (_options.Value.ApiKey);
        ListResponse<AssistantResponse> assistantResponses = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
        AssistantResponse? assistantResponse = assistantResponses.Items.SingleOrDefault(a => a.Name == summaryAssistantName) ?? 
                                               await client.AssistantsEndpoint.CreateAssistantAsync(new CreateAssistantRequest(
                                                       "gpt-4o",
                                                       summaryAssistantName, 
                                                       "", 
                                                       Prompts.AiConversationSummaryPrompt))
                                                   .ConfigureAwait(false);
        return assistantResponse.Id;
    }

    private async Task<string> GetAssumptionAssistantId()
    {
        string assumptionAssistantName = "AssumptionAssistant";
        using OpenAIClient client = new(_options.Value.ApiKey);
        ListResponse<AssistantResponse> assistantResponses = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
        AssistantResponse? assistantResponse = assistantResponses.Items.SingleOrDefault(a => a.Name == assumptionAssistantName) ??
            await client.AssistantsEndpoint.CreateAssistantAsync(new CreateAssistantRequest(
                "gpt-4o",
                assumptionAssistantName,
                "",
                Prompts.AiAssumptionPrompt, 
                responseFormat: ChatResponseFormat.Json))
            .ConfigureAwait(false);
        return assistantResponse.Id;
    }

    public async Task<Result<OpenAiImageResponse>> GetOpenAiImageResponse(string text)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetImage(text, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiAudioResponse>> GetOpenAiAudioResponse(string audioFilePath)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetTranscription(audioFilePath, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiSpeechResponse>> GetOpenAiSpeechResponse(string text)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetSpeech(text, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiVisionResponse(string text, string imageUrl, string? threadId)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetVision(text, imageUrl, threadId, client).ConfigureAwait(false);
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
        return await GetTextAnswer(threadId, client, assistantId).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        var threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
            { new OpenAI.Threads.Message(text) }, metadata: UserHelper.GetDictionaryWithUser(user))).ConfigureAwait(false);
        var assistantId = await _assistantHandler.GetOrCreateAssistantId(user, pilot, client).ConfigureAwait(false);

        return await GetTextAnswer(threadResponse.Id, client, assistantId).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetOpenAiPilotAssumption(textToBeEstimated, client);
    }

    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        
        var threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId);
        var lastMessageContent = (await threadResponse.ListMessagesAsync(new ListQuery(1))).Items.Single().PrintContent();
        Result<OpenAiResponse> conversationSummary = await GetConversationSummary(threadId, client, 4);
        string conversationMix = $"Previous Conversation:\n\nSummary: {conversationSummary.Value.Answer}\n\nLast Assistant Message:\n\n{lastMessageContent}";

        return await GetOpenAiPilotAssumption($"{conversationMix}\n\n{textToBeEstimated}", client);
    }

    private async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumption(string textToBeEstimated, OpenAIClient client)
    {
        Result<OpenAiPilotAssumptionResponse> openAiPilotAssumptionResponse;
        var threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
            { new OpenAI.Threads.Message($"Prompt: {textToBeEstimated}\r\nAvailable pilots:\r\n{JsonConvert.SerializeObject(_assistantHandler.PilotDescriptions)}") })).ConfigureAwait(false);
        Result<OpenAiResponse> result = await GetTextAnswer(threadResponse.Id, client, AssumptionAssistantId).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            openAiPilotAssumptionResponse = Result<OpenAiPilotAssumptionResponse>.Error(result.Errors.ToArray());
            return Result<OpenAiPilotAssumptionResponse>.Error(result.Errors.ToArray());
        }
        try
        {
            return new OpenAiPilotAssumptionResponse(JsonConvert.DeserializeObject<PilotAssumptionContainer>(result.Value.Answer));
        }
        catch (Exception e)
        {
            return Result<OpenAiPilotAssumptionResponse>.Error(e.Message);
        }
        finally
        {
            await client.ThreadsEndpoint.DeleteThreadAsync(threadResponse.Id).ConfigureAwait(false);
        }
    }

    public async Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetConversationSummary(threadId, client, messageCount);
    }

    internal async Task<AssistantResponse> GetAssistantResponseAsync(string assistantId)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }
    
    private async Task<Result<OpenAiResponse>> GetConversationSummary(string threadId, OpenAIClient client, int messageCount)
    {
        var threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId);
        var listMessagesAsync = await threadResponse.ListMessagesAsync(new ListQuery(messageCount));
        var conversation = string.Join("\n\n", listMessagesAsync.Items.Reverse().Select(r => $"{r.Role}: {r.PrintContent()}")); 
        threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
            { new OpenAI.Threads.Message(conversation) })).ConfigureAwait(false);
        var assistantId = await GetConversationSummaryAssistantId();
        return await GetTextAnswer(threadResponse.Id, client, assistantId);
    }
    
    private async Task<Result<OpenAiResponse>> GetTextAnswer(string threadId, OpenAIClient client, string assistantId)
    {
        var runResponse = await client.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId))
            .WaitForDone(_assistantHandler).ConfigureAwait(false);

        if (runResponse.Status != RunStatus.Completed)
            return Result<OpenAiResponse>.Error(
                $"Run {runResponse.Id} was ended with the status {Enum.GetName(typeof(RunStatus), runResponse.Status)}.");

        ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(limit: 1)).ConfigureAwait(false);
        var answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
        if (answer == null) return Result<OpenAiResponse>.Error("No answer was returned from the OpenAI API.");

        return new OpenAiResponse(answer, threadId);
    }

    private async Task<Result<OpenAiImageResponse>> GetImage(string text, OpenAIClient client)
    {
        try
        {
            var imageResult = (await client.ImagesEndPoint.GenerateImageAsync(new ImageGenerationRequest(text, Model.DallE_3)).ConfigureAwait(false))?.SingleOrDefault();
            if (imageResult == null) throw new ArgumentNullException($"ImageResult was null");
            return new OpenAiImageResponse(imageResult.Url);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return Result<OpenAiImageResponse>.Error($"Generate Image failed: {exception.Message}");
        }
    }

    private async Task<Result<OpenAiAudioResponse>> GetTranscription(string audioFilePath, OpenAIClient client)
    {
        try
        {
            AudioResponse audioResponse = await client.AudioEndpoint.CreateTranscriptionJsonAsync(new AudioTranscriptionRequest(audioFilePath)).ConfigureAwait(false);
            if (audioResponse == null) throw new ArgumentNullException($"AudioResponse was null");
            return new OpenAiAudioResponse(audioResponse.Text);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return Result<OpenAiAudioResponse>.Error($"Get Transcription failed: {exception.Message}");
        }
    }

    private async Task<Result<OpenAiSpeechResponse>> GetSpeech(string text, OpenAIClient client)
    {
        try
        {
            ReadOnlyMemory<byte> speechBytes = await client.AudioEndpoint.CreateSpeechAsync(new SpeechRequest(text)).ConfigureAwait(false);
            if (speechBytes.IsEmpty) throw new ArgumentNullException($"SpeechResponse was null");
            return new OpenAiSpeechResponse(speechBytes.ToArray());
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return Result<OpenAiSpeechResponse>.Error($"Get Speech failed: {exception.Message}");
        }
    }

    private async Task<Result<OpenAiResponse>> GetVision(string text, string url, string? threadId, OpenAIClient client)
    {
        try
        {
            var message = new Message(new[] { new Content(new ImageUrl(url)), new Content(text) });

            if (threadId is null)
            {
                var thread = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest([message]));
                threadId = thread.Id;
            }
            else
            {
                await client.ThreadsEndpoint.CreateMessageAsync(threadId, message);
            }

            var runResponse = await client.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(VisionAssistantId)).WaitForDone(_assistantHandler);
            ListResponse<MessageResponse> messagesResponse = await runResponse.ListMessagesAsync(new ListQuery(limit: 1)).ConfigureAwait(false);
            var answer = messagesResponse.Items.SingleOrDefault()?.PrintContent();
            if (answer == null) return Result<OpenAiResponse>.Error("No answer was returned from the OpenAI API.");
            return new OpenAiResponse(answer, threadId);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return Result<OpenAiResponse>.Error($"Get Vision Response failed: {exception.Message}");
        }
    }
}