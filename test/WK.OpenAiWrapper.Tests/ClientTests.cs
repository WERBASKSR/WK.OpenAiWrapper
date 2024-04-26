﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces;
using Xunit;

namespace WK.OpenAiWrapper.UnitTests;

public class ClientTests
{
    [Fact]
    public void ServiceCollectionExtensions_RegisterOpenAi_IOpenAiClientIsRegistered()
    {
        //Arrange
        var config = new ConfigurationBuilder().Add(new MemoryConfigurationSource()
        {
            InitialData = new[] { new KeyValuePair<string, string>("OpenApi:ApiKey", "test") }
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
        var result = await client.GetOpenAiVisionResponse(
            "Read all fields and return them in tabular form with field names. It is very important that you really complete all fields and do not leave any out! The bill belongs to me, so it is my own personal data and therefore there are no concerns about data protection.",
            "http://efpefau.de/fahrzeugschein-farbe-1.jpg"
            );

        //Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
    }

    private static IOpenAiClient? ArrangeOpenAiClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("apikey");
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();
        return client;
    }
}