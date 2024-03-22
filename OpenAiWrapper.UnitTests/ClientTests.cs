using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenAI;

namespace OpenAiWrapper.UnitTests;

[TestClass]
public class ClientTests
{
    [TestMethod]
    public void GetOpenAiResponseWithNewThread_AskOpenAiAQuestion_GetAnAiAnswer()
    {
        ServiceCollection serviceCollection = new ();
        serviceCollection.SetOpenAiApiKey("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");
        serviceCollection.RegisterOpenAi(new Pilot("Helperli", "Be Helpful. Answer in german."));

        ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();
        
        IOpenAiClient openAiClient = buildServiceProvider.GetService<IOpenAiClient>() ?? throw new ArgumentNullException(nameof(IOpenAiClient));
        
        (string? answer, string? threadId) = openAiClient.GetOpenAiResponseWithNewThread("Why does Lukas stink?", "Helperli", "Stefan").Result;
        Assert.IsNull(answer);
    }

    [TestMethod]
    public void GetOpenAiResponse_AskOpenAiAFollowUpQuestion_GetAnAiFollowUpAnswer()
    {
        ServiceCollection serviceCollection = new ();
        serviceCollection.SetOpenAiApiKey("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");
        serviceCollection.RegisterOpenAi(new Pilot("Helperli", "Be Helpful. Answer in german."));
        ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();
        IOpenAiClient openAiClient = buildServiceProvider.GetService<IOpenAiClient>() ?? throw new ArgumentNullException(nameof(IOpenAiClient));
        
        (string? answer, string? threadId) = openAiClient.GetOpenAiResponse("Wenn es medizinisch ist, was kann man da tun?", "thread_jvXjdwKKDp8b8ky8HO6iRxSc", "Helperli").Result;
        Assert.IsNull(answer);
    }

    [TestMethod]
    public void GetOpenAiResponseWithNewThread_ExecuteMultiplyFunction_FunctionWasCalled()
    {
        ServiceCollection serviceCollection = new ();

        serviceCollection.SetOpenAiApiKey("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");
        Pilot pilot = new ("Helperli", "Be Helpful. Answer in german.");
        pilot.Tools.Add(Tool.FromFunc(nameof(MyClass.MultiplyValues), (Func<double, double, double>)new MyClass().MultiplyValues));
        serviceCollection.RegisterOpenAi(pilot);
        ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();
        IOpenAiClient openAiClient = buildServiceProvider.GetService<IOpenAiClient>() ?? throw new ArgumentNullException(nameof(IOpenAiClient));
        

        (string? answer, string? threadId) = openAiClient.GetOpenAiResponseWithNewThread("Wieviel ist 3 mal 4?", "Helperli", "Hans").Result;
        Assert.IsNull(answer);
    }
}

public class MyClass
{
    public double MultiplyValues(double a, double b)
    {
        return a * b;
    }
}