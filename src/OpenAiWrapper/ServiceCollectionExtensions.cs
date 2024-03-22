using System.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenAiWrapper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetOpenAiApiKey(this IServiceCollection serviceCollection)
    {
        string? apiKey = ConfigurationManager.AppSettings.Get("OpenAiApiKey");
        if (apiKey == null) throw new MissingFieldException("OpenAiApiKey is missing in AppSettings");
        SetOpenAiApiKey(serviceCollection, apiKey);
        return serviceCollection;
    }

    public static IServiceCollection SetOpenAiApiKey(this IServiceCollection serviceCollection, string apiKey)
    {
        serviceCollection.AddSingleton<IOpenAiClient, Client>(p => new Client(p.GetService<AssistantHandler>(), apiKey));
        return serviceCollection;
    }

    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection, params Pilot[] pilots)
    {
        List<string> pilotNames = pilots.Select(p => p.Name).ToList();
        if (pilotNames.Distinct().Count() != pilotNames.Count) throw new ArgumentException("Pilot names must be unique.");

        foreach (Pilot pilot in pilots) serviceCollection.AddKeyedSingleton(pilot.Name, pilot);

        serviceCollection.AddSingleton<AssistantHandler>();
        return serviceCollection;
    }
}