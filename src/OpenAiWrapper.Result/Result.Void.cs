namespace OpenAiWrapper.Result;

public class Result : Result<Result>
{
    /// <summary>
    ///     Represents a successful operation without return type
    /// </summary>
    /// <returns>A Result</returns>
    public static Result Success()
    {
        return new Result();
    }

    /// <summary>
    ///     Represents a successful operation without return type
    /// </summary>
    /// <param name="successMessage">Sets the SuccessMessage property</param>
    /// <returns>A Result></returns>
    public static Result SuccessWithMessage(string successMessage)
    {
        return new Result { SuccessMessage = successMessage };
    }

    /// <summary>
    ///     Represents a successful operation and accepts a values as the result of the operation
    /// </summary>
    /// <param name="value">Sets the Value property</param>
    /// <returns>A Result<typeparamref name="T" /></returns>
    public static Result<T> Success<T>(T value)
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
    public static Result<T> Success<T>(T value, string successMessage)
    {
        return new Result<T>(value, successMessage);
    }

    /// <summary>
    ///     Represents an error that occurred during the execution of the service.
    ///     Error messages may be provided and will be exposed via the Errors property.
    /// </summary>
    /// <param name="errorMessages">A list of string error messages.</param>
    /// <returns>A Result</returns>
    public new static Result Error(params string[] errorMessages)
    {
        return new Result { Errors = errorMessages, IsSuccess = false };
    }

    /// <summary>
    ///     Represents an error that occurred during the execution of the service.
    ///     Sets the CorrelationId property to the provided value
    ///     Error messages may be provided and will be exposed via the Errors property.
    /// </summary>
    /// <param name="correlationId">Sets the CorrelationId property.</param>
    /// <param name="errorMessages">A list of string error messages.</param>
    /// <returns>A Result</returns>
    public static Result ErrorWithCorrelationId(string correlationId, params string[] errorMessages)
    {
        return new Result
        {
            CorrelationId = correlationId,
            Errors = errorMessages,
            IsSuccess = false
        };
    }
}