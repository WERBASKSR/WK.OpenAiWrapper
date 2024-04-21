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
using OpenAI.Audio;
using OpenAI.Images;
using OpenAI.Models;
using OpenAI.Chat;

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

    public async Task<Result<OpenAiResponse>> GetOpenAiVisionResponse(string text, string url)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await GetVision(text, url, client).ConfigureAwait(false);
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
            { new OpenAI.Threads.Message(text) }, UserHelper.GetDictionaryWithUser(user))).ConfigureAwait(false);
        var assistantId = await _assistantHandler.GetOrCreateAssistantId(user, pilot, client).ConfigureAwait(false);

        return await GetTextAnswer(threadResponse.Id, client, assistantId).ConfigureAwait(false);
    }

    internal async Task<AssistantResponse> GetAssistantResponseAsync(string assistantId)
    {
        using OpenAIClient client = new (_options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
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
            ImageResult? imageResult = (await client.ImagesEndPoint.GenerateImageAsync(new ImageGenerationRequest(text, Model.DallE_3)).ConfigureAwait(false))?.SingleOrDefault();
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

    private async Task<Result<OpenAiResponse>> GetVision(string text, string url, OpenAIClient client)
    {
        try
        {
            OpenAI.Chat.Message message = new (Role.User, new List<OpenAI.Chat.Content>
            {
                text,
                new ImageUrl(url, ImageDetail.Low)
            });
            var chatRequest = new ChatRequest(new []{ message }, model: Model.GPT4_Turbo);
            var response = await client.ChatEndpoint.GetCompletionAsync(chatRequest);
            var answer = response.FirstChoice.Message;
            if (answer == null) return Result<OpenAiResponse>.Error("No answer was returned from the OpenAI API.");

            return new OpenAiResponse(answer, "VisionCall");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return Result<OpenAiResponse>.Error($"Get Vision Response failed: {exception.Message}");
        }
    }
}