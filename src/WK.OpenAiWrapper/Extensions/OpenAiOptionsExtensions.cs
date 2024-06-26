using MoreLinq;
using OpenAI.Assistants;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;

namespace WK.OpenAiWrapper.Extensions;

internal static class OpenAiOptionsExtensions
{
    internal static Pilot? GetPilot(this OpenAiOptions options, string pilotName) =>
        options.Pilots.SingleOrDefault(p =>
            string.Equals(p.Name, pilotName, StringComparison.OrdinalIgnoreCase));

    internal static OpenAiOptions AddPilot(this OpenAiOptions options, Pilot pilot)
    {
        if (options.Pilots.Any(p => string.Equals(p.Name, pilot.Name, StringComparison.OrdinalIgnoreCase)))  throw new ArgumentException("PilotNames names must be unique.");
        options.Pilots.Add(pilot);
        ServiceCollectionExtensions.TransferToolBuilders([pilot]);
        ServiceCollectionExtensions.CreateToolResources([pilot]);
        return options;
    }
    
    internal static async Task<Pilot?> DeletePilotAsync(this OpenAiOptions options, string pilotName, IOpenAiClient client)
    {
        Pilot? pilot = options.GetPilot(pilotName);
        if (pilot == null) return pilot;
        
        options.Pilots.Remove(pilot);
        if (options.AssistantHandler == null) return pilot;
        
        var toDeleteAssistants = options.AssistantHandler.Assistants.Where(a => a.Value.Pilot == pilot)
            .Select(p => p.Value).ToList();
        toDeleteAssistants.ForEach(options.AssistantHandler.Assistants.RemoveValues);

        foreach (var toDeleteAssistant in toDeleteAssistants)
        {
            var assistantId = await options.AssistantHandler
                .GetOrCreateAssistantId(toDeleteAssistant.User, toDeleteAssistant.Pilot.Name, client)
                .ConfigureAwait(false);
            await client.DeleteAssistantAsync(assistantId).ConfigureAwait(false);
            options.AssistantHandler.AssistantIds.RemoveValues(assistantId);
        }

        return pilot;
    }
    
    internal static async Task<OpenAiOptions> UpdatePilotAsync(this OpenAiOptions options, Pilot pilot, IOpenAiClient client)
    {
        await DeletePilotAsync(options, pilot.Name, client).ConfigureAwait(false);
        AddPilot(options, pilot);
        return options;
    }
}