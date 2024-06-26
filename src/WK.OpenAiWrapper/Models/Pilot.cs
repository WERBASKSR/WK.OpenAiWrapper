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

    public string Name { get; set; } = Name;
    public string Instructions { get; set; } = Instructions;
    public string Description { get; set; } = Description;
    public string Model { get; set; } = Model;
    public bool JsonResponse { get; set; } = JsonResponse;

    internal void TransferToolBuildersToTools()
    {
        Tools.Clear();
        ToolFunctions.ForEach(t =>
        {
            Tools.Add(t.GenerateTool());
        });
    }

    internal void CreateToolResources()
    {
        if (!VectorStoreIds.Any()) return;
        
        var fileSearchTool = new Tool(Tool.FileSearch);
        if (Tools.All(t => t.Id != fileSearchTool.Id)) Tools.Add(fileSearchTool);
        ToolResources = null;
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