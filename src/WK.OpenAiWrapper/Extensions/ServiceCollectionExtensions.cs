using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoreLinq;
using OpenAI;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;

namespace WK.OpenAiWrapper.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection, string apiKey, params Pilot[] pilots)
    {
        serviceCollection.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = apiKey;
            options.Pilots = pilots;
        });
        
        TransferToolBuilders(pilots);
        ValidatePilots(pilots);
        Register(serviceCollection);
        return serviceCollection;
    }
    
    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection, IConfigurationRoot configuration, params Pilot[] pilots)
    {
        serviceCollection.Configure<OpenAiOptions>(configuration.GetRequiredSection(OpenAiOptions.SectionName));
        
        TransferToolBuilders(pilots);
        ValidatePilots(pilots);
        Register(serviceCollection);
        return serviceCollection;
    }
    
    private static void TransferToolBuilders(Pilot[] pilots) => pilots.ForEach(p => p.TransferToolBuildersToTools());
    
    private static void ValidatePilots(Pilot[] pilots)
    {
        //Check pilotNames are unique
        var pilotNames = pilots.Select(p => p.Name).ToList();
        if (pilotNames.Distinct().Count() != pilotNames.Count) throw new ArgumentException("Pilot names must be unique.");
    }
    
    private static void Register(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped(p => new OpenAIClient(
            new OpenAIAuthentication(p.GetRequiredService<IOptions<OpenAiOptions>>().Value.ApiKey)));
        
        serviceCollection.AddSingleton<IOpenAiClient, Client>();
    }
}