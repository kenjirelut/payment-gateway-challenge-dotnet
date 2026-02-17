namespace PaymentGateway.Api.Models.Requests;

public record PostPaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    int Cvv);