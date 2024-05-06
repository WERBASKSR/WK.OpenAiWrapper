using System.Reflection;
using OpenAI;

namespace WK.OpenAiWrapper.Helpers;

public record ToolFunction(string MethodFullName, string? Description = null)
{
    public Tool GenerateTool()
    {
        MethodInfo methodInfoGetOrCreateFunction = typeof(Function)
            .GetMethod("GetOrCreateFunction", BindingFlags.NonPublic | BindingFlags.Static);
        ToolFunctionInfo toolFunctionInfo = new (MethodFullName);
        MethodInfo methodInfo = toolFunctionInfo.GetMethodInfo();
        object instance = methodInfo.IsStatic ? null : Activator.CreateInstance(methodInfo.DeclaringType);
            
        //(string name, string description, MethodInfo method, object instance = null)
        Function function = (Function)methodInfoGetOrCreateFunction.Invoke(null, [toolFunctionInfo.MethodName, Description ?? toolFunctionInfo.Description, methodInfo, instance]);
        return new Tool(function);
    }
}