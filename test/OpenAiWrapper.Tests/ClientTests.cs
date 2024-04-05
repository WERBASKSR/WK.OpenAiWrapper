using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenAiWrapper.Extensions;

namespace OpenAiWrapper.UnitTests;

[TestClass]
public class ClientTests
{
    [TestMethod]
    public void GetAssistaHandler()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi();
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var assistantHandler = buildServiceProvider.GetService<AssistantHandler>();
    }
}