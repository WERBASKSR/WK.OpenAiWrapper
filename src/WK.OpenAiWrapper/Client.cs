﻿using OpenAI;
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
using OpenAI.Files;
using OpenAI.Images;
using OpenAI.Models;
using OpenAI.VectorStores;
using Message = OpenAI.Threads.Message;

namespace WK.OpenAiWrapper;

internal class Client : IOpenAiClient
{
    private readonly string _summaryAssistantId;
    private readonly string _assumptionAssistantId;
    internal readonly IOptions<OpenAiOptions> Options;
    private readonly AssistantHandler _assistantHandler;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static Client Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Client(IOptions<OpenAiOptions> options)
    {
        Options = options;
        _assistantHandler = new(options);
        using OpenAIClient client = new (Options.Value.ApiKey);
        _assumptionAssistantId = client.GetAssumptionAssistant().GetAwaiter().GetResult()?.Id ?? throw new ArgumentNullException(
                                    $"{nameof(_assumptionAssistantId)}: The AssumptionAssistant could not be retrieved or created. ");
                
        _summaryAssistantId = client.GetSummaryAssistant().GetAwaiter().GetResult()?.Id ??
                             throw new ArgumentNullException(
                                 $"{nameof(_summaryAssistantId)}: The AssumptionAssistant could not be retrieved or created. ");
        
        Instance = this;
    }

    public async Task<Result<OpenAiImageResponse>> GetOpenAiImageResponse(string text)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await GetImage(text, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiAudioResponse>> GetOpenAiAudioResponse(string audioFilePath)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await GetTranscription(audioFilePath, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiSpeechResponse>> GetOpenAiSpeechResponse(string text)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await GetSpeech(text, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null, string? imageUrl = null)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        
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

        var contentList = new List<Content> { new(text) };
        if (imageUrl != null) contentList.Add(new Content(ContentType.ImageUrl, imageUrl));
        
        await threadResponse.CreateMessageAsync(new Message(contentList)).ConfigureAwait(false);
        return await GetTextAnswer(threadId, client, assistantId).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user, string? imageUrl = null)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        
        var contentList = new List<Content>{ new (text) };
        if (imageUrl != null) contentList.Add(new Content(ContentType.ImageUrl, imageUrl));
        
        var threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
            { new Message(contentList) }, metadata: UserHelper.GetDictionaryWithUser(user))).ConfigureAwait(false);
        var assistantId = await _assistantHandler.GetOrCreateAssistantId(user, pilot, client).ConfigureAwait(false);

        return await GetTextAnswer(threadResponse.Id, client, assistantId).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionResponse(string textToBeEstimated)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await GetOpenAiPilotAssumption(textToBeEstimated, client).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumptionWithConversationResponse(string textToBeEstimated, string threadId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        
        var threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId).ConfigureAwait(false);
        var lastMessageContent = (await threadResponse.ListMessagesAsync(new ListQuery(1)).ConfigureAwait(false)).Items.Single().PrintContent();
        Result<OpenAiResponse> conversationSummary = await GetConversationSummary(threadId, client, 4);
        string conversationMix = $"Previous Conversation:\n\nSummary: {conversationSummary.Value.Answer}\n\nLast Assistant Message:\n\n{lastMessageContent}";

        return await GetOpenAiPilotAssumption($"{conversationMix}\n\n{textToBeEstimated}", client);
    }

    public async Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await GetConversationSummary(threadId, client, messageCount).ConfigureAwait(false);
    }

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(string[] filePaths, string vectorStoreName)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);
            return await UploadToVectorStore(filePaths, vectorStore.Id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(string[] filePaths, string vectorStoreId)
    {
        try
        {
            var list = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(Options.Value.ApiKey);

            foreach (var filePath in filePaths)
            {
                FileResponse fileResponse = await client.FilesEndpoint.UploadFileAsync(filePath, "assistants").ConfigureAwait(false);
                await client.VectorStoresEndpoint.CreateVectorStoreFileAsync(vectorStoreId, fileResponse).ConfigureAwait(false);
                list.Add((fileResponse.FileName, fileResponse.Id));
            }

            return new OpenAiMultipleFilesVectorStoreResponse(vectorStoreId, list);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreName)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);

            return await UploadToVectorStore(files, vectorStore.Id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreId)
    {
        try
        {
            var list = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(Options.Value.ApiKey);

            foreach (var file in files)
            {
                var fileResponseResult = await UploadStreamToVectorStore(new MemoryStream(file.FileBytes), file.FileName, vectorStoreId).ConfigureAwait(false);
                if (fileResponseResult.IsSuccess) list.Add((file.FileName, fileResponseResult.Value.FileId!));
            }

            return new OpenAiMultipleFilesVectorStoreResponse(vectorStoreId, list);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToNewVectorStore(Stream fileStream, string fileName, string vectorStoreName)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);
            return await UploadStreamToVectorStore(fileStream, fileName, vectorStore.Id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToVectorStore(Stream fileStream, string fileName, string vectorStoreId)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            FileResponse fileResponse = await client.FilesEndpoint.UploadFileAsync(new FileUploadRequest(fileStream, fileName, "assistants")).ConfigureAwait(false);
            await client.VectorStoresEndpoint.CreateVectorStoreFileAsync(vectorStoreId, fileResponse).ConfigureAwait(false);
            await fileStream.DisposeAsync().ConfigureAwait(false);
            return new OpenAiVectorStoreResponse(vectorStoreId, fileResponse.Id);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreByName(string fileName, string vectorStoreId)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var fileListAll = await client.FilesEndpoint.ListFilesAsync("assistants").ConfigureAwait(false);
            var fileIds = fileListAll
                .Where(r => string.Equals(r.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id);
            var vectorStoreFilesAll = await client.VectorStoresEndpoint.ListVectorStoreFilesAsync(vectorStoreId);
            var vectorStoreFile = vectorStoreFilesAll.Items.SingleOrDefault(r => fileIds.Contains(r.Id));
            if (vectorStoreFile != null)
            {
                return await DeleteFileInVectorStoreById(vectorStoreFile.Id, vectorStoreId).ConfigureAwait(false);
            }
            return new OpenAiVectorStoreResponse(vectorStoreId, vectorStoreFile?.Id);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiVectorStoreResponse>> DeleteFileInVectorStoreById(string fileId, string vectorStoreId)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            await client.VectorStoresEndpoint.DeleteVectorStoreFileAsync(vectorStoreId, fileId).ConfigureAwait(false);
            await client.FilesEndpoint.DeleteFileAsync(fileId).ConfigureAwait(false);
            return new OpenAiVectorStoreResponse(vectorStoreId, fileId);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    internal async Task<AssistantResponse> GetAssistantResponseAsync(string assistantId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }

    private async Task<Result<OpenAiPilotAssumptionResponse>> GetOpenAiPilotAssumption(string textToBeEstimated, OpenAIClient client)
    {
        ThreadResponse? threadResponse = null;
        try
        {
            threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
                { new Message($"Prompt: {textToBeEstimated}\r\nAvailable pilots:\r\n{JsonConvert.SerializeObject(_assistantHandler.PilotDescriptions)}") })).ConfigureAwait(false);
            Result<OpenAiResponse> result = await GetTextAnswer(threadResponse.Id, client, _assumptionAssistantId).ConfigureAwait(false);
            
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
    
    private async Task<Result<OpenAiResponse>> GetConversationSummary(string threadId, OpenAIClient client, int messageCount)
    {
        ThreadResponse? threadResponse = null;
        try
        {
            threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId);
            var listMessagesAsync = await threadResponse.ListMessagesAsync(new ListQuery(messageCount)).ConfigureAwait(false);
            var conversation = string.Join("\n\n", listMessagesAsync.Items.Reverse().Select(r => $"{r.Role}: {r.PrintContent()}")); 
            threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
                { new Message(conversation) })).ConfigureAwait(false);
            Result<OpenAiResponse> result = await GetTextAnswer(threadResponse.Id, client, _summaryAssistantId).ConfigureAwait(false);

            return !result.IsSuccess ? Result<OpenAiResponse>.Error(result.Errors.ToArray()) : result;
        }
        catch (Exception e)
        {
            return Result<OpenAiResponse>.Error(e.Message);
        }
        finally
        {
            if (threadResponse?.Id != null) await client.ThreadsEndpoint.DeleteThreadAsync(threadResponse.Id).ConfigureAwait(false);
        }
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
}