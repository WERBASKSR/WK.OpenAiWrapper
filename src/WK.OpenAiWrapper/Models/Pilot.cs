using MoreLinq;
using OpenAI;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

public record Pilot(string Name, string Instructions, string Description)
{
    private string? _model = "gpt-4o";

    public string? Model
    {
        get
        {
            //Bugfix until the nuget 'OpenAI-DotNet' update is available and the gpt-4o can be used
            //Todo: Delete this condition again as soon as the nuget update is available
            if (_model == "gpt-4o")
            {
                return "gpt-4-turbo";
            }

            return _model;
        }
        set => _model = value;
    }

    public ICollection<Tool> Tools { get; } = new HashSet<Tool>();
    public ICollection<ToolFunction> ToolFunctions { get; } = new HashSet<ToolFunction>();

    internal void TransferToolBuildersToTools() => ToolFunctions.ForEach(t => Tools.Add(t.GenerateTool()));
}