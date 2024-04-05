using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenAiWrapper.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetOpenAiApiKey(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        SetOpenAiApiKey(serviceCollection,
            configuration["OpenAi:ApiKey"] ??
            throw new InvalidOperationException("OpenAi:ApiKey is missing in configuration"));
        return serviceCollection;
    }

    public static IServiceCollection SetOpenAiApiKey(this IServiceCollection serviceCollection, string apiKey)
    {
        serviceCollection.AddSingleton<IOpenAiClient, Client>(p =>
            new Client(p.GetRequiredService<AssistantHandler>(), apiKey));
        return serviceCollection;
    }

    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection, params Pilot[] pilots)
    {
        var pilotNames = pilots.Select(p => p.Name).ToList();
        if (pilotNames.Distinct().Count() != pilotNames.Count)
            throw new ArgumentException("Pilot names must be unique.");

        foreach (var pilot in pilots) serviceCollection.AddKeyedSingleton(pilot.Name, pilot);

        serviceCollection.AddSingleton<AssistantHandler>();
        return serviceCollection;
    }
}