using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using WK.OpenAiWrapper.Options;

namespace WK.OpenAiWrapper.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection, string apiKey,
        params Pilot[] pilots)
    {
        serviceCollection.Configure<OpenAi>(options =>
        {
            options.ApiKey = apiKey;
            options.Pilots = pilots;
        });
        serviceCollection.AddScoped<OpenAIClient>(p =>
            new OpenAIClient(new OpenAIAuthentication(p.GetRequiredService<IOptions<OpenAi>>().Value.ApiKey)));
        serviceCollection.AddSingleton<IOpenAiClient, Client>(p =>
            new Client(p.GetRequiredService<AssistantHandler>(),
                p.GetRequiredService<OpenAIClient>()));
        var pilotNames = pilots.Select(p => p.Name).ToList();
        if (pilotNames.Distinct().Count() != pilotNames.Count)
            throw new ArgumentException("Pilot names must be unique.");

        foreach (var pilot in pilots) serviceCollection.AddKeyedSingleton(pilot.Name, pilot);
        return serviceCollection;
    }


    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection,
        IConfiguration configuration, params Pilot[] pilots)
    {
        serviceCollection.Configure<OpenAi>(configuration.GetRequiredSection(nameof(OpenAi)));
        serviceCollection.AddSingleton<IOpenAiClient, Client>(p =>
            new Client(p.GetRequiredService<AssistantHandler>(),
                p.GetRequiredService<OpenAIClient>()));
        var pilotNames = pilots.Select(p => p.Name).ToList();
        if (pilotNames.Distinct().Count() != pilotNames.Count)
            throw new ArgumentException("Pilot names must be unique.");

        foreach (var pilot in pilots) serviceCollection.AddKeyedSingleton(pilot.Name, pilot);

        serviceCollection.AddSingleton<AssistantHandler>();
        return serviceCollection;
    }
}