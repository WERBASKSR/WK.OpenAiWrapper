﻿using System.Reflection;
using OpenAI;

namespace WK.OpenAiWrapper.Helpers;

public record ToolFunction(string MethodFullName, string? Description = null)
{
    public Tool GenerateTool()
    {
        ConstructorInfo constructorInfo = typeof(Function)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(c => c.GetParameters().Length > 3);
        ToolFunctionInfo toolFunctionInfo = new (MethodFullName);
        MethodInfo methodInfo = toolFunctionInfo.GetMethodInfo();
        object instance = methodInfo.IsStatic ? null : Activator.CreateInstance(methodInfo.DeclaringType);
            
        //(string name, string description, MethodInfo method, object instance = null)
        Function function = (Function)constructorInfo.Invoke(new []{ toolFunctionInfo.MethodName, Description ?? toolFunctionInfo.Description, methodInfo, instance });
        return new Tool(function);
    }
}