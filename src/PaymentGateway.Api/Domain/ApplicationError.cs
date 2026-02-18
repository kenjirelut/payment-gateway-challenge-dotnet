namespace PaymentGateway.Api.Domain;

public record ApplicationError (ErrorType ErrorType, string Description);