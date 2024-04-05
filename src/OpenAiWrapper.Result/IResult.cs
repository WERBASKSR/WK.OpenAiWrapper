namespace OpenAiWrapper.Result;

public interface IResult
{
    IEnumerable<string> Errors { get; }
    IEnumerable<ValidationError> ValidationErrors { get; }
    Type ValueType { get; }
    object GetValue();
}