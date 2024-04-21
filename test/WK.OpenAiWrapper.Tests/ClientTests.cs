using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Models;
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
        
        //Assert
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();
        
        Assert.NotNull(client);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiResponseWithNewThread_AiResponseAnAnswer()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("apikey", 
            new Pilot("pilot1", "Be helpful.", "gpt-3.5-turbo-0125"));
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();

        var text = "Hello, what is 42?";
        var pilot = "pilot1";
        var user = "user1";

        var result = await client.GetOpenAiResponseWithNewThread(text, pilot, user);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        Assert.NotEmpty(result.Value.ThreadId);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiResponse_AiResponseAnAnswer()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("apikey");
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();

        var text = "Tell me more about 42.";
        var threadId = "threadId";

        var result = await client.GetOpenAiResponse(text, threadId);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Answer);
        Assert.NotEmpty(result.Value.ThreadId);
    }
    
    [Fact]
    public async Task IOpenAiClient_GetOpenAiImageResponse_AiImageInResponse()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("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");
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();

        var text = "A dog on the moon in an astronaut suit.";

        var result = await client.GetOpenAiImageResponse(text);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Url);
    }
}