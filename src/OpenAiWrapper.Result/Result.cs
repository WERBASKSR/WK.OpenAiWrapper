using System.Text.Json.Serialization;

namespace OpenAiWrapper.Result;

public class Result<T>
{
    protected Result()
    {
    }

    public Result(T value)
    {
        Value = value;
    }

    protected internal Result(T value, string successMessage) : this(value)
    {
        SuccessMessage = successMessage;
    }

    //public static implicit operator Result<T>(Result result) => new (default(T))
    //{
    //    IsSuccess = result.IsSuccess,
    //    Errors = result.Errors,
    //    SuccessMessage = result.SuccessMessage,
    //    CorrelationId = result.CorrelationId,
    //    ValidationErrors = result.ValidationErrors,
    //};

    [JsonInclude] public T Value { get; init; }

    [JsonInclude] public bool IsSuccess { get; protected set; } = true;

    [JsonInclude] public string SuccessMessage { get; protected set; } = string.Empty;

    [JsonInclude] public string CorrelationId { get; protected set; } = string.Empty;

    [JsonIgnore] public Type ValueType => typeof(T);

    [JsonInclude] public IEnumerable<string> Errors { get; protected set; } = Array.Empty<string>();

    [JsonInclude]
    public IEnumerable<ValidationError> ValidationErrors { get; protected set; } = Array.Empty<ValidationError>();

    /// <summary>
    ///     Returns the current value.
    /// </summary>
    /// <returns></returns>
    public object GetValue()
    {
        return Value;
    }

    public static implicit operator T(Result<T> result)
    {
        return result.Value;
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    ///     Represents a successful operation and accepts a values as the result of the operation
    /// </summary>
    /// <param name="value">Sets the Value property</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    /// <summary>
    ///     Represents a successful operation and accepts a values as the result of the operation
    ///     Sets the SuccessMessage property to the provided value
    /// </summary>
    /// <param name="value">Sets the Value property</param>
    /// <param name="successMessage">Sets the SuccessMessage property</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T> Success(T value, string successMessage)
    {
        return new Result<T>(value, successMessage);
    }

    /// <summary>
    ///     Represents an error that occurred during the execution of the service.
    ///     Error messages may be provided and will be exposed via the Errors property.
    /// </summary>
    /// <param name="errorMessages">A list of string error messages.</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T> Error(params string[] errorMessages)
    {
        return new Result<T> { Errors = errorMessages, IsSuccess = false };
    }
}