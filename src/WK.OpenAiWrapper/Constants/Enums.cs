using System.Runtime.Serialization;

namespace WK.OpenAiWrapper;

public enum FilePurposeEnum
{
    [EnumMember(Value = "assistants")]
    Assistants,

    [EnumMember(Value = "assistants_output")]
    AssistantsOutput,

    [EnumMember(Value = "batch")]
    Batch,

    [EnumMember(Value = "batch_output")]
    BatchOutput,

    [EnumMember(Value = "fine-tune")]
    FineTune,

    [EnumMember(Value = "fine-tune-results")]
    FineTuneResults,

    [EnumMember(Value = "vision")]
    Vision
}