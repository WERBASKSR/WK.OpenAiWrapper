using System.Reflection;
using OpenAI;

namespace WK.OpenAiWrapper.Helpers
{
    public record ToolBuilder(string MethodFullName, string Description)
    {
        public Tool GenerateTool()
        {
            ConstructorInfo constructorInfo = typeof(Function)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(c => c.GetParameters().Length > 3);
            MethodNameInfo methodNameInfo = new (MethodFullName);
            MethodInfo methodInfo = methodNameInfo.GetMethodInfo();
            object instance = methodInfo.IsStatic ? null : Activator.CreateInstance(methodInfo.DeclaringType);
            
            //(string name, string description, MethodInfo method, object instance = null)
            Function function = (Function)constructorInfo.Invoke(new []{ methodNameInfo.MethodName, Description, methodInfo, instance });
            return new Tool(function);
        }
    }
}
