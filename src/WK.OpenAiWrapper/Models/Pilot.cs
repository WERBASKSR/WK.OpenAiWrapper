using MoreLinq;
using OpenAI;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

public record Pilot(string Name, string Instructions, string Model = "gpt-4-turbo")
{
    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once ReturnTypeCanBeEnumerable.Global
    public ICollection<Tool> Tools { get; } = new HashSet<Tool>();
    public ICollection<ToolFunction> ToolBuilders { get; } = new HashSet<ToolFunction>();

    internal void TransferToolBuildersToTools() => ToolBuilders.ForEach(t => Tools.Add(t.GenerateTool()));
}