using System.Text.Json.Serialization;

namespace WK.OpenAiWrapper.Result;

public class Result<T>
{
    protected Result() { }

    public Result(T? value) => Value = value; 

    protected internal Result(T? value, string successMessage) : this(value) => SuccessMessage = successMessage; 

    [JsonInclude] public T? Value { get; init; }

    [JsonInclude] public bool IsSuccess { get; protected set; } = true;

    [JsonInclude] public string SuccessMessage { get; protected set; } = string.Empty;

    [JsonInclude] public string CorrelationId { get; protected set; } = string.Empty;

    [JsonIgnore] public Type ValueType => typeof(T);

    [JsonInclude] public List<string> Errors { get; protected set; } = new ();

    [JsonInclude] public List<ValidationError> ValidationErrors { get; protected set; } = new ();

    public object? GetValue() => Value ?? default;

    public static implicit operator T?(Result<T?>? result) => (result != null ? result.Value : default) ?? default;

    public static implicit operator Result<T?>(T? value) => new (value);

    /// <summary>
    ///     Represents a successful operation and accepts a values as the result of the operation
    /// </summary>
    /// <param name="value">Sets the Value property</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T?> Success(T? value) => new(value);

    /// <summary>
    ///     Represents a successful operation and accepts a values as the result of the operation
    ///     Sets the SuccessMessage property to the provided value
    /// </summary>
    /// <param name="value">Sets the Value property</param>
    /// <param name="successMessage">Sets the SuccessMessage property</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T?> Success(T? value, string successMessage) => new(value, successMessage);

    /// <summary>
    ///     Represents an error that occurred during the execution of the service.
    ///     Error messages may be provided and will be exposed via the Errors property.
    /// </summary>
    /// <param name="errorMessages">A list of string error messages.</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T?> Error(params string[] errorMessages)
    {
        var result = new Result<T?> { IsSuccess = false, SuccessMessage = $"Error {errorMessages?.FirstOrDefault() ?? string.Empty}" };
        if (errorMessages != null) result.Errors.AddRange(errorMessages);
        return result;
    }

    public override string ToString() => IsSuccess ? SuccessMessage : string.Join(Environment.NewLine, Errors);
}