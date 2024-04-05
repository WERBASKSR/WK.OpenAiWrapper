using OpenAI;

namespace OpenAiWrapper;

public record Pilot(string Name, string Instructions, string Model = "gpt-4")
{
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public ICollection<Tool> Tools { get; } = new HashSet<Tool>();
}