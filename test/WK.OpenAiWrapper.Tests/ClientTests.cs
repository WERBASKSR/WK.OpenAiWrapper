using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using OpenAI;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;
using Xunit;

namespace WK.OpenAiWrapper.Tests;

public class ClientTests
{
    [Fact]
    public void ServiceCollectionExtensions_RegisterOpenAi_IOpenAiClientIsRegistered()
    {
        //Arrange
        var config = new ConfigurationBuilder().Add(new MemoryConfigurationSource()
        {
            InitialData = [new KeyValuePair<string, string>("OpenAi:ApiKey", "test")]
        }).Build();
        var serviceCollection = new ServiceCollection();
        
        //Act
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();
        
        //Assert
        
        Assert.NotNull(client);
    }
    
    [Fact]
    public void ServiceCollectionExtensions_RegisterOpenAiWithPilotInConfigurationAndParameter_AllPilotsAreRegistered()
    {
        //Arrange
        var json = @"{""OpenAi:ApiKey"": ""test"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant.""
                        }
                    ]
                }";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        
        var serviceCollection = new ServiceCollection();
        Pilot pilot = new ("Post Configured Pilot", "You are a crazy AI.", "A crazy Pilot.");
        
        //Act
        serviceCollection.RegisterOpenAi(config, pilot);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
        
        //Assert
        Assert.NotNull(client);
        Assert.True(client.Options.Value.Pilots.Count == 2);
    }
    
    [Fact]
    public void ServiceCollectionExtensions_RegisterOpenAiWithToolFunctions_FunctionsAreInTools()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        
        //Assert
        Assert.NotNull(client);
        Assert.True(client.Options.Value.Pilots.Count == 1);
        Assert.True(client.Options.Value.Pilots[0].Tools.Count == 1);
    }
    
    [Fact]
    public async void IOpenAiClient_GetOpenAiResponseWithNewThreadAndWithFunctionCall_AiResponseAnAnswer()
    {
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        string user = $"UnitTest_{Guid.NewGuid()}";

        var text = "Erstelle eine Zusammenfassung aus dem WorkItem 35879";
        var pilot = "Master";
        
        //Act
        var result = await client.GetOpenAiResponseWithNewThread(text, pilot, user);
        
        //Assert
        Assert.NotNull(client);
        Assert.True(client.Options.Value.Pilots.Count == 1);
        Assert.True(client.Options.Value.Pilots[0].Tools.Count == 1);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiResponseWithNewThread_AiResponseAnAnswer()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        string user = $"UnitTest_{Guid.NewGuid()}";

        var text = "Hello, what is 42?";
        var pilot = "pilot1";
        
        //Act
        var result = await client.GetOpenAiResponseWithNewThread(text, pilot, user);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        Assert.NotEmpty(result.Value.ThreadId);
    }
    
    [Fact]
    public async void IOpenAiClient_GetOpenAiPilotAssumptionResponse_WeatherPilotAssumptionAsJsonInAnswer()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        var text = "What is the weather like in Paris?";
        
        //Act
        var result = await client.GetOpenAiPilotAssumptionResponse(text);
        
            //Assert Arrange
            int weatherPercentage = result.Value.PilotAssumptionContainer.PilotAssumptions.Single(p => p.PilotName == "Weather")
                .ProbabilityInPercent;
            int masterPercentage = result.Value.PilotAssumptionContainer.PilotAssumptions.Single(p => p.PilotName == "Master")
                .ProbabilityInPercent;
        
        //Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value.PilotAssumptionContainer.PilotAssumptions);
        Assert.True(result.Value.PilotAssumptionContainer.PilotAssumptions.Count == 2);
        Assert.True(weatherPercentage > masterPercentage);
    }
    
    [Fact]
    public async void IOpenAiClient_GetOpenAiPilotAssumptionWithConversationResponse_WeatherPilotAssumptionAsJsonInAnswer()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        var text = "What is the weather like in Paris?";
        
        //Act
        var result = await client.GetOpenAiPilotAssumptionWithConversationResponse(text, "thread_CYpL8a4ECD1V9lT53rpnkY9R");
        
            //Assert Arrange
            int weatherPercentage = result.Value.PilotAssumptionContainer.PilotAssumptions.Single(p => p.PilotName == "Weather")
                .ProbabilityInPercent;
            int masterPercentage = result.Value.PilotAssumptionContainer.PilotAssumptions.Single(p => p.PilotName == "Master")
                .ProbabilityInPercent;
        
        //Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value.PilotAssumptionContainer.PilotAssumptions);
        Assert.True(result.Value.PilotAssumptionContainer.PilotAssumptions.Count == 2);
        Assert.True(weatherPercentage > masterPercentage);
    }
    
    [Fact]
    public async void IOpenAiClient_GetConversationSummaryResponse_ReturnedASummaryOfAConversation()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        
        //Act
        Result<OpenAiResponse> result = await client.GetConversationSummaryResponse("thread_Or9Zvn8tXnfCOOwpqwMKDupu");
        

        //Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Answer);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiResponse_AiResponseAnAnswer()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        var text = "Tell me more about 42.";
        var threadId = "thread_jusM149NfdO1fqibBw1j7HDd";
        
        //Act
        var result = await client.GetOpenAiResponse(text, threadId);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        Assert.NotEmpty(result.Value.ThreadId);
    }
    
    [Fact]
    public async Task IOpenAiClient_DeleteFileInVectorStore_FileIsDeleted()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        var fileName = "Result.Void.cs.txt";
        var storeId = "vs_fozloSRtH9k8KrYRa1znvXCF";
        
        //Act
        var result = await client.DeleteFileInVectorStoreByName(fileName, storeId);
        
        //Assert
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task IOpenAiClient_UploadFileInVectorStore_FileIsUploaded()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        var fileName = @"C:\Users\sbechtel\OneDrive - Werbas GmbH\Desktop\openai store\Result.Void.cs.txt";
        var storeId = "vs_fozloSRtH9k8KrYRa1znvXCF";
        
        //Act
        var result = await client.UploadToVectorStore([fileName], storeId);
        
        //Assert
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiImageResponse_AiImageInResponse()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        var text = "A dog on the moon in an astronaut suit.";
        
        //Act
        var result = await client.GetOpenAiImageResponse(text);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Url);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiAudioResponse_TranscriptionInResponse()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        string mp3FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ElevenLabs_2024-04-21.mp3");
        if (!File.Exists(mp3FilePath)) throw new FileNotFoundException(mp3FilePath);
        
        //Act
        var result = await client.GetOpenAiAudioResponse(mp3FilePath);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Text);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiSpeechResponse_SpeechInResponse()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        string mp3FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "speech.mp3");
        
        var text = "Ein Hund auf dem Mond in einem Astronautenanzug.";
        
        //Act
        var result = await client.GetOpenAiSpeechResponse(text);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.AudioFileBytes);
        
        //Manual Check
        File.WriteAllBytes(mp3FilePath, result.Value.AudioFileBytes);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiVisionResponse_VisionAnswerInResponse()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetService<IOpenAiClient>() as Client;
        using var openAiClient = serviceProvider.GetRequiredService<OpenAIClient>();
        string user = $"UnitTest_{Guid.NewGuid()}";


        //Act
        var result = await client.GetOpenAiResponseWithNewThread(
            """
             Read all fields and return them in tabular form with field names.
             It is very important that you really complete all fields and do not leave any out! 
             The bill belongs to me, so it is my own personal data and therefore there are no concerns about data protection.",
            """,
            "Master",
            user,
            ["http://domainname.de/fahrzeugschein-farbe-1.jpg"]
            );

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        
        await openAiClient.ThreadsEndpoint.DeleteThreadAsync(result.Value.ThreadId);
        await openAiClient.AssistantsEndpoint.DeleteAssistantAsync(result.Value.AssistantId);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiResponseWithImageAttachment_AnswerInResponse()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetRequiredService<IOpenAiClient>();
        using var openAiClient = serviceProvider.GetRequiredService<OpenAIClient>();
        string user = $"UnitTest_{Guid.NewGuid()}";


        //Act
        var result = await client.GetOpenAiResponseWithNewThread(
            "Warum ist das lustig?",
            "Master",
            user,
            ["http://domainname.de/ai_testpic.jpg"]
        );

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        
        await openAiClient.ThreadsEndpoint.DeleteThreadAsync(result.Value.ThreadId);
        await openAiClient.AssistantsEndpoint.DeleteAssistantAsync(result.Value.AssistantId);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiResponseWithTxtAttachment_AnswerInResponse()
    {
        //Arrange
        IServiceProvider serviceProvider = ArrangeOpenAiClient();
        var client = serviceProvider.GetRequiredService<IOpenAiClient>();
        using var openAiClient = serviceProvider.GetRequiredService<OpenAIClient>();
        string user = $"UnitTest_{Guid.NewGuid()}";

        //Act
        var result = await client.GetOpenAiResponseWithNewThread(
            "Erstelle eine sehr kurze Zusammenfassung in einem kurzen Absatz.",
            "Master",
            user,
            ["http://domainname.de/ai_example.txt"]
        );

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);

        await openAiClient.ThreadsEndpoint.DeleteThreadAsync(result.Value.ThreadId);
        await openAiClient.AssistantsEndpoint.DeleteAssistantAsync(result.Value.AssistantId);
    }

    private static IServiceProvider ArrangeOpenAiClient()
    {
        var json = @"{""OpenAi:ApiKey"": ""testapikey"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant."",
                            ""Description"": ""This is a Fallback assistant for all general questions and tasks."",
                            ""ToolFunctions"": [
                              {
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.AdoCom.GetWIInfos""
                              },
                              {
                                ""Type"": ""file_search""
                              }]
                        },
                        {
                            ""Name"": ""Weather"",
                            ""Instructions"": ""You are a weather expert."",
                            ""Description"": ""This is a weather assistant for weather questions."",
                            ""ToolFunctions"": [
                              {
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.WeatherCalls.GetWeather"",
                                ""Description"": ""Retrieves information about a weather in a location.""
                              }]
                        }
                    ]
                }";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi(config, [new Pilot("pilot1", "Be helpful", "Helpful Ai")]);
        return serviceCollection.BuildServiceProvider();
    }
}