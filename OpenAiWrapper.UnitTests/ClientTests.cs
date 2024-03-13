using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenAiWrapper.UnitTests
{
    [TestClass]
    public class ClientTests
    {
        [TestMethod]
        public void GetAssistaHandler()
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.RegisterOpenAi();
            ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();
            AssistantHandler assistantHandler = buildServiceProvider.GetService<AssistantHandler>();
        }
    }
}
