﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
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
                                ""MethodFullName"": ""WK.OpenAiWrapper.Tests.AdoCom.GetWIInfos""
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
        IOpenAiClient? client = ArrangeOpenAiClient();

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
        IOpenAiClient? client = ArrangeOpenAiClient();

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
            "http://domainname.de/fahrzeugschein-farbe-1.jpg"
            );

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
    }

    private static IOpenAiClient? ArrangeOpenAiClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("", 
            new Pilot("pilot1", "Be helpful", "Helpful Ai"));
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();
        return client;
    }
}