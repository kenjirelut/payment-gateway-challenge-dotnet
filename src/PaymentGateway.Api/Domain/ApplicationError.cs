namespace PaymentGateway.Api.Domain;

public record ApplicationError(ErrorType ErrorType, string Description)
{
    private const string BankServiceUnavailable = "Bank Service Unavailable";
    private const string InvalidBankRequest = "Invalid request sent to Bank";
    private const string PaymentNotFoundMessage = "Payment not found";
    public static ApplicationError BankError(string? description = null) => new(ErrorType.SubServiceUnavailable, description ?? BankServiceUnavailable);
    public static ApplicationError BankBadRequest() => Internal(InvalidBankRequest);
    public static ApplicationError Internal (string description) => new(ErrorType.Internal, description);
    public static ApplicationError PaymentNotFound() => new(ErrorType.NotFound,  PaymentNotFoundMessage);
    

}

