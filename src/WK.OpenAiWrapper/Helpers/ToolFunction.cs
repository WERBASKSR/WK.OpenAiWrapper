using System.Reflection;
using OpenAI;

namespace WK.OpenAiWrapper.Helpers;

public record ToolFunction(string MethodFullName, string? Description = null)
{
    public Tool GenerateTool()
    {
        MethodInfo methodInfo = new ToolFunctionInfo(MethodFullName).GetMethodInfo();
        return methodInfo.IsStatic ? Tool.GetOrCreateTool(methodInfo.DeclaringType, methodInfo.Name) 
            : Tool.GetOrCreateTool(Activator.CreateInstance(methodInfo.DeclaringType), methodInfo.Name);
    }
}