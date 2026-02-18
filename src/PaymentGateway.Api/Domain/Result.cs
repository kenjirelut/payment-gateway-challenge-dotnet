namespace PaymentGateway.Api.Domain;

public record Result
{
    protected Result(bool isSuccess, ApplicationError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public ApplicationError? Error { get; }
    
    public static Result Success() => new(true, null);
    public static Result Failure(ApplicationError applicationError) => new(false, applicationError ?? throw new ArgumentNullException(nameof(applicationError)));

    public static implicit operator Result(ApplicationError applicationError) => Failure(applicationError);
}

public record Result<T> : Result
{
    public T? Value { get; }
    
    private Result(T value) : base(true, null) => Value = value;
    private Result(ApplicationError applicationError) : base(false, applicationError) {}
    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(ApplicationError error) => new(error);
}