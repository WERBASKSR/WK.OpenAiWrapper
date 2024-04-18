using System.ComponentModel.DataAnnotations;

namespace WK.OpenAiWrapper.Options;

public class OpenAi
{
    [Required(AllowEmptyStrings = false)]
    public string ApiKey { get; set; } = string.Empty;

    public ICollection<Pilot>? Pilots;
}