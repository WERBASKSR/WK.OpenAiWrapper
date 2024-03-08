using OpenAI;

namespace OpenAiWrapper;

public record Pilot(string Name, string Instructions, string Model = "gpt-4")
{
    public ICollection<Tool> Tools { get; } = new HashSet<Tool>();
}