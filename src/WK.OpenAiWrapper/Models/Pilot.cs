using MoreLinq;
using OpenAI;
using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Models;

public record Pilot(string Name, string Instructions, string Description, string Model = "gpt-4o", bool JsonResponse = false)
{
    public ICollection<string> VectorStoreIds { get; } = new HashSet<string>();
    
    internal ToolResources? ToolResources { get; set; }

    public ICollection<Tool> Tools { get; } = new HashSet<Tool>();
    
    public ICollection<ToolFunction> ToolFunctions { get; } = new HashSet<ToolFunction>();

    internal void TransferToolBuildersToTools() => ToolFunctions.ForEach(t => Tools.Add(t.GenerateTool()));
    
    internal void CreateToolResources()
    {
        if (!VectorStoreIds.Any()) return;
        foreach (string vectorStoreId in VectorStoreIds)
        {
            if (ToolResources?.FileSearch?.VectorStoreIds == null)
            {
                ToolResources = new ToolResources(new FileSearchResources(vectorStoreId));
                continue;
            }
            ((List<string>)ToolResources.FileSearch.VectorStoreIds).Add(vectorStoreId);
        }
    }
}