namespace WK.OpenAiWrapper.Models;

internal record PilotDescription(string Name, string Instructions, string Model, HashSet<FunctionDescription> FunctionDescriptions);