using System.Reflection;
using OpenAI;

namespace WK.OpenAiWrapper.Helpers;

public record ToolFunction(string? MethodFullName = null, string? Description = null, string Type = ToolFunction.Function)
{
    private const string ToolSearch = "file_search";
    private const string CodeInterpreter = "code_interpreter";
    private const string Function = "function";
    
    public Tool GenerateTool()
    {
        switch (Type.ToLower())
        {
            case ToolSearch: return Tool.FileSearch;
            case CodeInterpreter: return Tool.CodeInterpreter;
            case Function:
                var toolFunctionInfo = new ToolFunctionInfo(MethodFullName);
                var methodInfo = toolFunctionInfo.GetMethodInfo();
                return methodInfo.IsStatic ? Tool.GetOrCreateTool(methodInfo.DeclaringType, methodInfo.Name, toolFunctionInfo.Description) 
                    : Tool.GetOrCreateTool(Activator.CreateInstance(methodInfo.DeclaringType), methodInfo.Name, toolFunctionInfo.Description);
                break;
            default:
                throw new NotImplementedException(Type);
        }
    }
}