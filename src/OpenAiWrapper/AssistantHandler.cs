using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Assistants;

namespace OpenAiWrapper;

public class AssistantHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentBag<Assistant> _assistants = new ();

    public AssistantHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public CreateAssistantRequest GetCreateAssistantRequest(string user, string pilotName)
    {
        Assistant? assistant = _assistants.SingleOrDefault(a => a == (user, pilotName));
        if (assistant != null) return assistant.CreateAssistantRequest;

        Pilot pilot = _serviceProvider.GetKeyedService<Pilot>(pilotName) ?? throw new NotImplementedException($"{pilotName} is not registered in ServiceCollection");
        assistant = new Assistant(user, pilot);
        _assistants.Add(assistant);

        return assistant.CreateAssistantRequest;
    }
}