using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces.Clients;
using WK.OpenAiWrapper.Models;
using Xunit;

namespace WK.OpenAiWrapper.Tests;

public class ServiceCollectionTests
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
        serviceCollection.AddScoped<IMyAiService, MyAiService>();
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;
        
        //Assert
        Assert.NotNull(client);
        Assert.True(client.Options.Value.Pilots.Count == 2);
    }

    class MyAiService : IMyAiService
    {
        private readonly IOpenAiClient _client;

        public MyAiService(IOpenAiClient client)
        {
            _client = client;
        }
    }
    interface IMyAiService
    {
        
    }
}