using MoreLinq;
using OpenAI;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

public record Pilot(string Name, string Instructions, string Description)
{
    public string? Model { get; set; } = "gpt-4o";

    public ICollection<Tool> Tools { get; } = new HashSet<Tool>();
    
    public ICollection<ToolFunction> ToolFunctions { get; } = new HashSet<ToolFunction>();

    internal void TransferToolBuildersToTools() => ToolFunctions.ForEach(t => Tools.Add(t.GenerateTool()));
}