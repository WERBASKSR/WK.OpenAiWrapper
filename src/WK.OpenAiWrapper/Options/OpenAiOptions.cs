using System.ComponentModel.DataAnnotations;
using WK.OpenAiWrapper.Models;

namespace WK.OpenAiWrapper.Options;

public class OpenAiOptions
{
    internal const string SectionName = "OpenAi";
    [Required(AllowEmptyStrings = false)]
    public string ApiKey { get; set; } = string.Empty;

    public ICollection<Pilot>? Pilots;
}