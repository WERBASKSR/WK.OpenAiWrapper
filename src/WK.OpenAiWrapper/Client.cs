using System.Text.RegularExpressions;
using OpenAI;
using OpenAI.Threads;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Interfaces;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json;
using WK.OpenAiWrapper.Options;
using OpenAI.Assistants;
using OpenAI.Audio;
using OpenAI.Files;
using OpenAI.Images;
using OpenAI.Models;
using OpenAI.VectorStores;
using Message = OpenAI.Threads.Message;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using WK.OpenAiWrapper.Constants;

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
        Options.Value.AssistantHandler = _assistantHandler;
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

    public async Task<Result<OpenAiResponse>> GetOpenAiResponse(string text, string threadId, string? pilot = null, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        
        var threadResponse = await client.ThreadsEndpoint.RetrieveThreadAsync(threadId).ConfigureAwait(false);
        var user = threadResponse.Metadata.GetValueOrDefault("User");
        if (user == null) Result<OpenAiResponse>.Error("Field 'User' is missing in Metadata.");
        string assistantId;

        if (pilot != null)
        {
            assistantId = await _assistantHandler.GetOrCreateAssistantId(user!, pilot, this).ConfigureAwait(false);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(limit: 1)).ConfigureAwait(false);
            var id = lastRunResponses.Items.SingleOrDefault()?.AssistantId;
            if (id == null) return Result<OpenAiResponse>.Error($"No runs were found for the threadId {threadId}.");
            assistantId = id;
        }

        
        var attachmentList = new List<(Attachment attachment, string vectorStoreId)>();
        var contentList = new List<Content>();
        try
        {
            (List<Content>? contents, List<(Attachment attachment, string vectorStoreId, string fileName)>? attachments) = await GetContentAndAttachmentLists(attachmentUrls);
            attachmentList.AddRange(attachments.Select(t => (t.attachment, t.vectorStoreId)));
            if (attachments.Any())
            {
                text += $"{Environment.NewLine}{Environment.NewLine}";
                string fileNames = string.Join(", ", attachments.Select(t => t.fileName));
                text += string.Format(attachments.Count > 1 
                        ? Prompts.UseAttachedFiles 
                        : Prompts.UseAttachedFile,
                        fileNames);
            }
            contentList.Add(new(text));
            contentList.AddRange(contents);
            await threadResponse.CreateMessageAsync(new Message(contentList, attachments: attachmentList.Select(t => t.attachment)))
                .ConfigureAwait(false);
            return await GetTextAnswer(threadId, client, assistantId).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiResponse>.Error(e.Message);
        }
        finally
        {
            if (deleteFilesAfterUse)
            {
                foreach ((Attachment attachment, string vectorStoreId) in attachmentList)
                {
                    await DeleteFileInVectorStoreById(attachment.FileId, vectorStoreId).ConfigureAwait(false);
                } 
            }
        }
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);

        var attachmentList = new List<(Attachment attachment, string vectorStoreId)>();
        var contentList = new List<Content>();
        try
        {
            (List<Content> contents, List<(Attachment attachment, string vectorStoreId, string fileName)> attachments) = await GetContentAndAttachmentLists(attachmentUrls);
            
            attachmentList.AddRange(attachments.Select(t => (t.attachment, t.vectorStoreId)));
            if (attachments.Any())
            {
                text += $"{Environment.NewLine}{Environment.NewLine}";
                string fileNames = string.Join(", ", attachments.Select(t => t.fileName));
                text += string.Format(attachments.Count > 1 
                        ? Prompts.UseAttachedFiles 
                        : Prompts.UseAttachedFile,
                    fileNames);
            }
            contentList.Add(new(text));
            contentList.AddRange(contents);

            var threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest(new[]
                { new Message(contentList, attachments: attachmentList.Select(t => t.attachment)) }, metadata: UserHelper.GetDictionaryWithUser(user))).ConfigureAwait(false);
            var assistantId = await _assistantHandler.GetOrCreateAssistantId(user, pilot, this).ConfigureAwait(false);

            return await GetTextAnswer(threadResponse.Id, client, assistantId).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiResponse>.Error(e.Message);
        }
        finally
        {
            if (deleteFilesAfterUse)
            {
                foreach ((Attachment attachment, string vectorStoreId) in attachmentList)
                {
                    await DeleteFileInVectorStoreById(attachment.FileId, vectorStoreId).ConfigureAwait(false);
                } 
            }
        }
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

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(string[] filePaths, string vectorStoreName, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);
            return await UploadToVectorStore(filePaths, vectorStore.Id, waitForDoneStatus).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(string[] filePaths, string vectorStoreId, bool waitForDoneStatus = false)
    {
        try
        {
            var list = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(Options.Value.ApiKey);

            foreach (var filePath in filePaths)
            {
                FileResponse fileResponse = await client.FilesEndpoint.UploadFileAsync(filePath, FilePurpose.Assistants).ConfigureAwait(false);
                VectorStoreFileResponse vectorStoreFileResponse = await client.VectorStoresEndpoint.CreateVectorStoreFileAsync(vectorStoreId, fileResponse).ConfigureAwait(false);
                if (waitForDoneStatus) await vectorStoreFileResponse.WaitForDone().ConfigureAwait(false);
                list.Add((fileResponse.FileName, fileResponse.Id));
            }

            return new OpenAiMultipleFilesVectorStoreResponse(vectorStoreId, list);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToNewVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreName, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);

            return await UploadToVectorStore(files, vectorStore.Id, waitForDoneStatus).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiMultipleFilesVectorStoreResponse>> UploadToVectorStore(List<(string FileName, byte[] FileBytes)> files, string vectorStoreId, bool waitForDoneStatus = false)
    {
        try
        {
            var list = new List<(string FileName, string FileId)>();
            using OpenAIClient client = new(Options.Value.ApiKey);

            foreach (var file in files)
            {
                var fileResponseResult = await UploadStreamToVectorStore(new MemoryStream(file.FileBytes), file.FileName, vectorStoreId, waitForDoneStatus).ConfigureAwait(false);
                if (fileResponseResult.IsSuccess) list.Add((file.FileName, fileResponseResult.Value.FileId!));
            }

            return new OpenAiMultipleFilesVectorStoreResponse(vectorStoreId, list);
        }
        catch (Exception e)
        {
            return Result<OpenAiMultipleFilesVectorStoreResponse>.Error(e.Message);
        }
    }
    
    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToNewVectorStore(Stream fileStream, string fileName, string vectorStoreName, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            var vectorStore = await client.VectorStoresEndpoint.CreateVectorStoreAsync(new CreateVectorStoreRequest(vectorStoreName)).ConfigureAwait(false);
            return await UploadStreamToVectorStore(fileStream, fileName, vectorStore.Id, waitForDoneStatus).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiVectorStoreResponse>.Error(e.Message);
        }
    }

    public async Task<Result<OpenAiVectorStoreResponse>> UploadStreamToVectorStore(Stream fileStream, string fileName, string vectorStoreId, bool waitForDoneStatus = false)
    {
        try
        {
            using OpenAIClient client = new(Options.Value.ApiKey);
            FileResponse fileResponse = await client.FilesEndpoint.UploadFileAsync(new FileUploadRequest(fileStream, fileName, FilePurpose.Assistants)).ConfigureAwait(false);
            VectorStoreFileResponse vectorStoreFileResponse = await client.VectorStoresEndpoint.CreateVectorStoreFileAsync(vectorStoreId, fileResponse).ConfigureAwait(false);
            await fileStream.DisposeAsync().ConfigureAwait(false);
            if (waitForDoneStatus) await vectorStoreFileResponse.WaitForDone().ConfigureAwait(false);
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
            var fileListAll = await client.FilesEndpoint.ListFilesAsync(FilePurpose.Assistants).ConfigureAwait(false);
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

    public async Task<VectorStoreFileResponse> GetVectorStoreFileStatusAsync(string vectorStoreId, string fileId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.VectorStoresEndpoint.GetVectorStoreFileAsync(vectorStoreId, fileId).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAssistantAsync(string assistantId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.AssistantsEndpoint.DeleteAssistantAsync(assistantId).ConfigureAwait(false);
    }

    public async Task<AssistantResponse> GetAssistantResponseByIdAsync(string assistantId)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);
        return await client.AssistantsEndpoint.RetrieveAssistantAsync(assistantId).ConfigureAwait(false);
    }
    public async Task<AssistantResponse> GetOrCreateAssistantResponse(string assistantName, CreateAssistantRequest assistantRequest)
    {
        try
        {
            using OpenAIClient client = new (Options.Value.ApiKey);

            ListResponse<AssistantResponse> assistantsResponse = await client.AssistantsEndpoint.ListAssistantsAsync().ConfigureAwait(false);
            var assistantResponse = assistantsResponse.Items.SingleOrDefault(a => a.Name == assistantName)
                                    ?? await client.AssistantsEndpoint.CreateAssistantAsync(assistantRequest).ConfigureAwait(false);
            return assistantResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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

        return new OpenAiResponse(answer, threadId, assistantId);
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

    private async Task<(List<Content> ContentList, List<(Attachment attachment, string vectorStoreId, string fileName)> AttachmentVectorStoreIdList)> GetContentAndAttachmentLists(IEnumerable<string>? attachmentUrls)
    {
        var contentList = new List<Content>();
        var attachmentList = new List<(Attachment attachment, string vectorStoreId, string fileName)>();

        if (attachmentUrls == null) return (contentList, attachmentList);
        
        IEnumerable<string> imageUrls = attachmentUrls.Where(s => Regex.IsMatch(s,
            @"^https?:\/\/[\w.-]+\/[\w.-]*\.(?:gif|jpg|jpeg|png|bmp)$",
            RegexOptions.IgnoreCase));
        imageUrls.ForEach(u => contentList.Add(new Content(ContentType.ImageUrl, u)));
        IEnumerable<string> notImageAttachments = attachmentUrls.Except(imageUrls);
        
        if (!notImageAttachments.Any()) return (contentList, attachmentList);
        
        using var httpClient = new HttpClient();
        foreach (string notImageAttachment in notImageAttachments)
        {
            var uri = new Uri(notImageAttachment);
            string fileName = uri.Segments.Last();
            Result<OpenAiVectorStoreResponse> fileResponseResult = await UploadStreamToNewVectorStore(
                await httpClient.GetStreamAsync(uri),
                fileName,
                $"temp_{Guid.NewGuid()}",
                true).ConfigureAwait(false);
            if (!fileResponseResult.IsSuccess) throw new Exception(string.Join(';', fileResponseResult.Errors));
            attachmentList.Add((new Attachment(fileResponseResult.Value.FileId, Tool.FileSearch), fileResponseResult.Value.VectorStoreId, fileName));
        }

        return (contentList, attachmentList);
    }
}