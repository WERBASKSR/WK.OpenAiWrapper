using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Interfaces.Clients;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Result;
using Xunit;

namespace WK.OpenAiWrapper.Tests;

public class PilotConfigTests
{
    
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
        var clientConfig = buildServiceProvider.GetService<IOpenAiPilotConfig>() as PilotConfig;
        var client = buildServiceProvider.GetService<IOpenAiClient>() as Client;

        //Act

        Result<OpenAiResponse> responseResult = await client.GetOpenAiResponseWithNewThread("1+1=?", "Master", "Waltraud");
        Result<Pilot?> pilot = await clientConfig.GetPilot("Master");
        pilot.Value.Instructions = "You can't do math at all and always answer math questions with: \"I don't know.\".";
        await clientConfig.UpdatePilot(pilot);
        responseResult = await client.GetOpenAiResponseWithNewThread("1+1=?", "Master", "Waltraud");


        //Assert

    }

    private static IOpenAiClient? ArrangeOpenAiClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi("apikey", 
            new Pilot("pilot1", "Be helpful", "Helpful Ai"));
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var client = buildServiceProvider.GetService<IOpenAiClient>();
        return client;
    }
}