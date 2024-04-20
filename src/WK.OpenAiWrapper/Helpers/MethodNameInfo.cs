using MoreLinq;
using System.Reflection;

namespace WK.OpenAiWrapper.Helpers;

internal class MethodNameInfo
{
    public MethodNameInfo(string methodFullName)
    {
        List<string> methodFullNameParts = methodFullName.Split('.').ToList();
        methodFullNameParts.ForEach(s => 
            NameSpaceLayers.Add(NameSpaceLayers.Count == 0 ? s : $"{NameSpaceLayers.Last()}.{s}"));
        NameSpaceLayers = NameSpaceLayers.SkipLast(1).ToHashSet();
        NameSpaceLayers.Remove(NameSpaceLayers.Last());
        MethodName = methodFullNameParts.Last();
        methodFullNameParts.Remove(MethodName);
        ClassName = methodFullNameParts.Last();
        methodFullNameParts.Remove(ClassName);
        NameSpaceName = string.Join('.', methodFullNameParts);
    }
    
    public string MethodName { get; set; }
    public string ClassName { get; set; }
    public string NameSpaceName { get; set; }
    public HashSet<string> NameSpaceLayers { get; set; } = new();

    public MethodInfo GetMethodInfo()
    {
        Type type = GetClassType();
        MethodInfo? methodInfo = type.GetMethods(BindingFlags.Public
                                                            | BindingFlags.NonPublic
                                                            | BindingFlags.Instance
                                                            | BindingFlags.Static
                                                            | BindingFlags.CreateInstance
                                                            | BindingFlags.IgnoreCase
                                                            | BindingFlags.InvokeMethod)
                                            .Where(m => m.Name == MethodName)
                                            .OrderBy(m => m.GetParameters().Length)
                                            .FirstOrDefault();
        
        return methodInfo ?? throw new MissingMethodException($"The method {MethodName} was not found in the class {ClassName}.");
    }
    
    private Type GetClassType()
    {
        string fullClassName = $"{NameSpaceName}.{ClassName}";
        Type? type = Type.GetType(fullClassName);
        if (type != null) return type;
            
        string currentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        List<Assembly> currentAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        Assembly assembly = null;
        NameSpaceLayers.ForEach(s =>
        {
            IEnumerable<Assembly> assemblies = currentAssemblies.Where(a => a.FullName.StartsWith(s));
            if (assemblies.Count() == 1) assembly = assemblies.Single();
        });

        if (assembly == null)
        {
            NameSpaceLayers.ForEach(s =>
            {
                string potentialAssemblyPath = Path.Combine(currentDomainBaseDirectory, s + ".dll");
                if (!File.Exists(potentialAssemblyPath)) return;
                assembly = AppDomain.CurrentDomain.Load(Assembly.LoadFile(potentialAssemblyPath).GetName());
            });
        }

        if (assembly == null)
            throw new DllNotFoundException($"No DLL was found that points to the namespace: {NameSpaceName}");

        return assembly.GetType(fullClassName) ?? throw new TypeLoadException($"The type {fullClassName} could not be created.");
    }
}