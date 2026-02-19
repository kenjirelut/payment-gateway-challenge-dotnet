namespace PaymentGateway.Api.Domain;

public enum ErrorType
{
    Internal,
    SubServiceUnavailable,
    Validation,
    NotFound,
}