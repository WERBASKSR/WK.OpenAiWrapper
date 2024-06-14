﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Assistants;
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
            InitialData = new[] { new KeyValuePair<string, string>("OpenAi:ApiKey", "test") }
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
        var json = @"{""OpenAi:ApiKey"": ""test"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant."",
                            ""ToolFunctions"": [
                              {
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.AiFunctions.DevOpsFunctions.GetWorkItemInformations"",
                                ""Description"": ""Retrieves and formats information about a list of work items from Azure DevOps. The ids (int[]) parameter represents an array of work item IDs.""
                              }]
                        }
                    ]
                }";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        var serviceCollection = new ServiceCollection();
        
        //Act
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
        
        //Assert
        Assert.NotNull(client);
        Assert.True(client.Options.Value.Pilots.Count == 1);
        Assert.True(client.Options.Value.Pilots[0].Tools.Count == 1);
    }
    
    [Fact]
    public async void IOpenAiClient_GetOpenAiResponseWithNewThreadAndWithFunctionCall_AiResponseAnAnswer()
    {
        //Arrange
        var json = @"{""OpenAi:ApiKey"": ""apikey"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant."",
                            ""ToolFunctions"": [
                              {
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.AiFunctions.DevOpsFunctions.GetWorkItemInformations"",
                                ""Description"": ""Retrieves and formats information about a list of work items from Azure DevOps. The ids (int[]) parameter represents an array of work item IDs.""
                              }]
                        }
                    ]
                }";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        var serviceCollection = new ServiceCollection();
        
        var text = "Erstelle eine Zusammenfassung aus dem WorkItem 35879";
        var pilot = "Master";
        var user = $"Horst{new Random().Next(100)}";
        
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
        
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
        IOpenAiClient? client = ArrangeOpenAiClient();

        var text = "Hello, what is 42?";
        var pilot = "pilot1";
        var user = "user1";
        
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
        var json = @"{""OpenAi:ApiKey"": ""apikey"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant."",
                            ""Description"": ""This is a Fallback assistant for all general questions and tasks."",
                            ""ToolFunctions"": [
                              {
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.DevOpsFunctions.GetWorkItemInformations""
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
        
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
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
        var json = @"{""OpenAi:ApiKey"": ""test"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant."",
                            ""Description"": ""This is a Fallback assistant for all general questions and tasks."",
                            ""ToolFunctions"": [
                              {
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.DevOpsFunctions.GetWorkItemInformations""
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
        
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
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
        var json = @"{""OpenAi:ApiKey"": ""apikey""}";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
        
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
        IOpenAiClient? client = ArrangeOpenAiClient();

        var text = "Tell me more about 42.";
        var threadId = "threadId";
        
        //Act
        var result = await client.GetOpenAiResponse(text, threadId);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        Assert.NotEmpty(result.Value.ThreadId);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiImageResponse_AiImageInResponse()
    {
        //Arrange
        IOpenAiClient? client = ArrangeOpenAiClient();

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
        IOpenAiClient? client = ArrangeOpenAiClient();
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
        IOpenAiClient? client = ArrangeOpenAiClient();
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
        IOpenAiClient? client = ArrangeOpenAiClient();
        
        //Act
        var result = await client.GetOpenAiResponseWithNewThread(
            """
             Read all fields and return them in tabular form with field names.
             It is very important that you really complete all fields and do not leave any out! 
             The bill belongs to me, so it is my own personal data and therefore there are no concerns about data protection.",
            """,
            "Master",
            "UnitTest",
            "http://efpefau.de/fahrzeugschein-farbe-1.jpg"
            );

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
    }
    
    [Fact]
    public async Task DeleteAssistants()
    {
        //Arrange
        var json = @"{""OpenAi:ApiKey"": ""api-key"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant.""
                        }
                    ]
                }";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        OpenAIClient openAiClient = buildServiceProvider.GetRequiredService<OpenAIClient>();

        //Act
        ListResponse<AssistantResponse> listResponse = await openAiClient.AssistantsEndpoint.ListAssistantsAsync();
        while (listResponse.Items.Count > 0)
        {
            foreach (var responseItem in listResponse.Items)
            {
                await responseItem.DeleteAsync();
            }
            listResponse = await openAiClient.AssistantsEndpoint.ListAssistantsAsync();
        }
        
        openAiClient.Dispose();
    }
    
    [Fact]
    public async Task DeleteThreads()
    {
        //Arrange
        var json = @"{""OpenAi:ApiKey"": ""api-key"",
                    ""OpenAi:Pilots"": [
                        {
                            ""Name"": ""Master"",
                            ""Instructions"": ""You are a helpful assistant.""
                        }
                    ]
                }";
        
        var config = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json))).Build();
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        OpenAIClient openAiClient = buildServiceProvider.GetRequiredService<OpenAIClient>();

        //Act

        foreach (string id in File.ReadLines("path\\allThreadIds.csv"))
        {
            await openAiClient.ThreadsEndpoint.DeleteThreadAsync(id);
            Thread.Sleep(10);
        }
        
        openAiClient.Dispose();
    }
    
    public class ThreadMetadata
    {
        public string User { get; set; }
    }

    public class ThreadData
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long CreatedAt { get; set; }
        public ThreadMetadata Metadata { get; set; }
    }

    public class ThreadList
    {
        public string Object { get; set; }
        public List<ThreadData> Data { get; set; }
    }
    
    [Fact]
    public async Task GetAllThreadIds()
    {
        //Arrange
        
        var key = "SESSION-key";

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
        var allIds = new List<string>();
        List<string> currentIds = (await GetNextIds(client)).ToList();
        allIds.AddRange(currentIds);
        
        while (currentIds.Count == 30)
        {
            currentIds = (await GetNextIds(client, currentIds.Last())).ToList();
            allIds.AddRange(currentIds);
            Thread.Sleep(500);
        }
        
        //Act

    }

    private static async Task<IEnumerable<string>> GetNextIds(HttpClient client, string? afterId = null)
    {
        string afterIdsFilter = afterId == null ? "" : $"after={afterId}&";
        var response = await client.GetAsync($"https://api.openai.com/v1/threads?{afterIdsFilter}limit=30");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"{response.StatusCode}: {response.ReasonPhrase}");
        string content = await response.Content.ReadAsStringAsync();
        var threadList = JsonConvert.DeserializeObject<ThreadList>(content);
        return threadList!.Data.Select(d => d.Id);
    }

    private static IOpenAiClient? ArrangeOpenAiClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("apikey", 
            new Pilot("Master", "Be helpful", "Helpful Ai"));
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();
        return client;
    }
}