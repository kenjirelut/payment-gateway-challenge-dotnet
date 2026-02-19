namespace PaymentGateway.Api.Domain;

public record ValidatedPaymentRequest(
    string CardNumber,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    IsoCurrency Currency,
    int Amount,
    string Cvv)
{
    public string ExpiryDateForBank => $"{ExpiryMonth:D2}/{ExpiryYear}";
}
