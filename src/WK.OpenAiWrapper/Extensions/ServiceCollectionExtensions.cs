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
            options.Pilots.AddRange(pilots);
        });
        
        TransferToolBuilders(pilots);
        CreateToolResources(pilots);
        ValidatePilots(pilots);
        RegisterOpenAiClient(serviceCollection);
        return serviceCollection;
    }
    
    public static IServiceCollection RegisterOpenAi(this IServiceCollection serviceCollection, IConfigurationRoot configuration, params Pilot[] pilots)
    {
        serviceCollection = serviceCollection.Configure<OpenAiOptions>(configuration.GetRequiredSection(OpenAiOptions.SectionName));
        serviceCollection = serviceCollection.PostConfigure<OpenAiOptions>(o =>
        {
            o.Pilots.AddRange(pilots);
            ValidatePilots(o.Pilots);
            TransferToolBuilders(o.Pilots);
            CreateToolResources(o.Pilots);
        });
        RegisterOpenAiClient(serviceCollection);
        return serviceCollection;
    }
    
    internal static void TransferToolBuilders(IEnumerable<Pilot> pilots) => pilots.ForEach(p => p.TransferToolBuildersToTools());
    
    internal static void CreateToolResources(IEnumerable<Pilot> pilots) => pilots.ForEach(p => p.CreateToolResources());
    
    internal static void ValidatePilots(IEnumerable<Pilot> pilots)
    {
        //Check pilotNames are unique
        var pilotNames = pilots.Select(p => p.Name).ToList();
        if (pilotNames.Distinct().Count() != pilotNames.Count) throw new ArgumentException("PilotNames names must be unique.");
    }
    
    private static void RegisterOpenAiClient(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped(p => new OpenAIClient(new OpenAIAuthentication(p.GetRequiredService<IOptions<OpenAiOptions>>().Value.ApiKey)));
        serviceCollection.AddSingleton<IOpenAiClient, Client>();
        serviceCollection.AddSingleton<IOpenAiClientConfig, ClientConfig>();
    }
}