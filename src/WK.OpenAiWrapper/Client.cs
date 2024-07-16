using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Helpers;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;
using WK.OpenAiWrapper.Constants;
using WK.OpenAiWrapper.Services;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Threads;
using OpenAI.Assistants;
using OpenAI.Audio;
using OpenAI.Images;
using OpenAI.Models;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Interfaces.Clients;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper;

internal partial class Client : IOpenAiClient
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal static Client Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    internal readonly IOptions<OpenAiOptions> Options;
    internal readonly IAssistantHandler AssistantHandler;

    public Client(IOptions<OpenAiOptions> options, IAssumptionService assumptionService, IStorageService storageService, ISummaryService summaryService, IAssistantService assistantService, IAssistantHandler assistantHandler)
    {
        Options = options;
        AssistantHandler = assistantHandler;
        AssistantService = assistantService;
        SummaryService = summaryService;
        StorageService = storageService;
        AssumptionService = assumptionService;
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
        
        AssistantResponse assistant;

        if (pilot != null)
        {
            assistant = await AssistantHandler.GetOrCreateAssistantResponse(user!, pilot).ConfigureAwait(false);
        }
        else
        {
            ListResponse<RunResponse> lastRunResponses = await threadResponse.ListRunsAsync(new ListQuery(limit: 1)).ConfigureAwait(false);
            var id = lastRunResponses.Items.SingleOrDefault()?.AssistantId;
            if (id == null) return Result<OpenAiResponse>.Error($"No runs were found for the threadId {threadId}.");
            assistant = await GetAssistantResponseByIdAsync(id).ConfigureAwait(false);
        }
        
        var attachments = new List<Attachment>();
        try
        {
            var message = new Message(text);
            if (attachmentUrls != null && attachmentUrls.Any())
            {
                (Message? messageWithAttachments, AssistantResponse newAssistant) = await GetMessageWithAttachment(text, attachmentUrls, assistant).ConfigureAwait(false);
                message = messageWithAttachments;
                assistant = newAssistant;
            }
            attachments.AddRange(message.Attachments ?? []);
            
            await threadResponse.CreateMessageAsync(message).ConfigureAwait(false);
            return await GetTextAnswer(threadId, client, assistant.Id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiResponse>.Error(e.Message);
        }
        finally
        {
            string? vectorStoreId = assistant.ToolResources?.FileSearch?.VectorStoreIds?.SingleOrDefault();
            if (deleteFilesAfterUse && vectorStoreId != null)
            {
                foreach (Attachment attachment in attachments)
                {
                    await DeleteFileInVectorStoreById(attachment.FileId, vectorStoreId).ConfigureAwait(false);
                } 
            }
        }
    }

    public async Task<Result<OpenAiResponse>> GetOpenAiResponseWithNewThread(string text, string pilot, string user, IEnumerable<string>? attachmentUrls = null, bool deleteFilesAfterUse = false)
    {
        using OpenAIClient client = new (Options.Value.ApiKey);

        var assistant = await AssistantHandler.GetOrCreateAssistantResponse(user, pilot).ConfigureAwait(false);
        var attachments = new List<Attachment>();
        try
        {
            var message = new Message(text);
            if (attachmentUrls != null && attachmentUrls.Any())
            {
                (Message? messageWithAttachments, AssistantResponse newAssistant) = await GetMessageWithAttachment(text, attachmentUrls, assistant).ConfigureAwait(false);
                message = messageWithAttachments;
                assistant = newAssistant;
            }
            attachments.AddRange(message.Attachments ?? []);
            
            ToolResources toolResources = null;
            if (attachments.Any()) 
                toolResources = new ToolResources(
                    new FileSearchResources(assistant.ToolResources.FileSearch.VectorStoreIds.Single()));
            
            var threadResponse = await client.ThreadsEndpoint.CreateThreadAsync(new CreateThreadRequest([message], toolResources, UserHelper.GetDictionaryWithUser(user))).ConfigureAwait(false);

            return await GetTextAnswer(threadResponse.Id, client, assistant.Id).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            return Result<OpenAiResponse>.Error(e.Message);
        }
        finally
        {
            string? vectorStoreId = assistant.ToolResources?.FileSearch?.VectorStoreIds?.SingleOrDefault();
            if (deleteFilesAfterUse && vectorStoreId != null)
            {
                foreach (Attachment attachment in attachments)
                {
                    await DeleteFileInVectorStoreById(attachment.FileId, vectorStoreId).ConfigureAwait(false);
                } 
            }
        }
    }

    internal async Task<Result<OpenAiResponse>> GetTextAnswer(string threadId, OpenAIClient client, string assistantId)
    {
        var runResponse = await client.ThreadsEndpoint.CreateRunAsync(threadId, new CreateRunRequest(assistantId))
            .WaitForDone(AssistantHandler).ConfigureAwait(false);

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
    
    private async Task<(Message, AssistantResponse)> GetMessageWithAttachment(string text, IEnumerable<string> attachmentUrls, AssistantResponse assistant)
    {
        string? vectorStoreId = assistant.ToolResources?.FileSearch?.VectorStoreIds?.SingleOrDefault();
        (List<Content> contents, List<(Attachment attachment, string fileName)> attachments, string newVectorStoreId) 
            = await StorageService.GetContentAndAttachmentLists(attachmentUrls, vectorStoreId);
            
        if (vectorStoreId == null) assistant = await AssistantService.ReplaceVectorStoreIdToAssistantByIdAsync(assistant.Id, newVectorStoreId);
        if (attachments.Any())
        {
            text += $"{Environment.NewLine}{Environment.NewLine}";
            string fileNames = string.Join(", ", attachments.Select(t => t.fileName));
            text += string.Format(attachments.Count > 1 
                    ? Prompts.UseAttachedFiles 
                    : Prompts.UseAttachedFile,
                fileNames);
        }
        contents.Insert(0,new(text));
        return (new Message(contents, attachments: attachments.Select(t => t.attachment)), assistant);
    }
}