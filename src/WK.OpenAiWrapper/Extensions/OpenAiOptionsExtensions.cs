using WK.OpenAiWrapper.Interfaces.Clients;
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
    
    internal static async Task<Pilot?> DeletePilotAsync(this OpenAiOptions options, string pilotName)
    {
        var client = IOpenAiClient.GetRequiredInstance();
        var pilot = options.GetPilot(pilotName);
        if (pilot == null) return pilot;
        
        options.Pilots.Remove(pilot);
        if (client.AssistantHandler == null) return pilot;
        
        var toDeleteAssistants = client.AssistantHandler.Assistants.Where(a => a.Value.Pilot == pilot)
            .Select(p => p.Value).ToList();
        toDeleteAssistants.ForEach(client.AssistantHandler.Assistants.RemoveValues);

        foreach (var toDeleteAssistant in toDeleteAssistants)
        {
            var assistantId = await client.AssistantHandler
                .GetOrCreateAssistantId(toDeleteAssistant.User, toDeleteAssistant.Pilot.Name)
                .ConfigureAwait(false);
            await client.DeleteAssistantAsync(assistantId).ConfigureAwait(false);
            client.AssistantHandler.AssistantIds.RemoveValues(assistantId);
        }

        return pilot;
    }
    
    internal static async Task<OpenAiOptions> UpdatePilotAsync(this OpenAiOptions options, Pilot pilot)
    {
        await DeletePilotAsync(options, pilot.Name).ConfigureAwait(false);
        AddPilot(options, pilot);
        return options;
    }
    
    internal static async Task<Pilot?> ModifyAssistantAsync(this OpenAiOptions options, string pilotName, string userName)
    {
        var client = IOpenAiClient.GetRequiredInstance();
        var pilot = options.GetPilot(pilotName);
        if (pilot == null) return pilot;
        
        pilot.TransferToolBuildersToTools();
        pilot.CreateToolResources();
        
        var assistantId = await client.AssistantHandler.GetOrCreateAssistantId(userName, pilotName);
        var assistant = client.AssistantHandler.GetCreateAssistant(userName, pilotName);
        assistant._createAssistantRequest = null;

        await client.ModifyAssistantResponseByIdAsync(assistantId, assistant.CreateAssistantRequest);

        return pilot;
    }
}