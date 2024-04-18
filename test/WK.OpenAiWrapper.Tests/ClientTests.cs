using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using WK.OpenAiWrapper.Extensions;
using Xunit;

namespace WK.OpenAiWrapper.UnitTests;

public class ClientTests
{
    [Fact]
    public void GetAssistantHandler()
    {
        var config = new ConfigurationBuilder().Add(new MemoryConfigurationSource()
        {
            InitialData = new[] { new KeyValuePair<string, string>("OpenApi:ApiKey", "test") }
        }).Build();


        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi(config);
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var assistantHandler = buildServiceProvider.GetService<AssistantHandler>();
    }
}