using WK.OpenAiWrapper.Models;

namespace WK.OpenAiWrapper.Extensions;

internal static class ModelExtensions
{
    public static IEnumerable<PilotDescription> ToPilotDescriptions(this IEnumerable<Pilot> pilots)
    {
        foreach (Pilot pilot in pilots)
        {
            yield return pilot.ToPilotDescription();
        }
    }
    public static PilotDescription ToPilotDescription(this Pilot pilot) =>
        new(pilot.Name, pilot.Instructions, pilot.Model,
            pilot.Tools.Select(t 
                => new FunctionDescription(t.Function.Name, t.Function.Description)).ToHashSet());
}