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
        
        Result<OpenAiResponse> result = openAiClient.GetOpenAiResponseWithNewThread("Why does Lukas stink?", "Helperli", "Stefan").Result;
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.Answer);
    }

    [TestMethod]
    public void GetOpenAiResponse_AskOpenAiAFollowUpQuestion_GetAnAiFollowUpAnswer()
    {
        ServiceCollection serviceCollection = new ();
        serviceCollection.SetOpenAiApiKey("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");
        serviceCollection.RegisterOpenAi(new Pilot("Helperli", "Be Helpful. Answer in german."));
        ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();
        IOpenAiClient openAiClient = buildServiceProvider.GetService<IOpenAiClient>() ?? throw new ArgumentNullException(nameof(IOpenAiClient));
        
        Result<OpenAiResponse> result = openAiClient.GetOpenAiResponse("Wenn es medizinisch ist, was kann man da tun?", "thread_jvXjdwKKDp8b8ky8HO6iRxSc", "Helperli").Result;
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.Answer);
    }

    [TestMethod]
    public void GetOpenAiResponseWithNewThread_ExecuteMultiplyFunction_FunctionWasCalled()
    {
        ServiceCollection serviceCollection = new ();

        serviceCollection.SetOpenAiApiKey("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");
        Pilot pilot = new ("Helperli", "Be Helpful. Answer in german.");
        Tool tool = Tool.GetOrCreateTool(new MyClass(), nameof(MyClass.GetFullnameForInitials), "This function knows all full names for all initials that could be requested.");
        pilot.Tools.Add(tool);
        serviceCollection.RegisterOpenAi(pilot);
        ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();
        IOpenAiClient openAiClient = buildServiceProvider.GetService<IOpenAiClient>() ?? throw new ArgumentNullException(nameof(IOpenAiClient));


        Result<OpenAiResponse> result = openAiClient.GetOpenAiResponseWithNewThread("Was ist der vollständige Name zu folgenden Initialen: LS?\r\nNutze die Funktion GetFullnameForInitials für die Beantwortung!", "Helperli", "Hans").Result;
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.Answer);
    }
}

public class MyClass
{
    public async Task<string> GetFullnameForInitials(string initials)
    {
        switch (initials)
        {
            case "LS": return "Lukilein Schachili";
            case "SB": return "Großmeister B";
            default: return "unknown";
        }
    }
}