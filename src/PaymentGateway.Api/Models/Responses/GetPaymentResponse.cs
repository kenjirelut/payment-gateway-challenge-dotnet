using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Models.Responses;

public record GetPaymentResponse
{
    public GetPaymentResponse(Guid id, string status, string cardNumberLastFour, int expiryMonth, int expiryYear, string currency, int amount)
    {
        Id = id;
        Status = status;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
    }

    public Guid Id { get; init; }
    public string Status { get; init; }
    public string CardNumberLastFour { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; }
    public int Amount { get; init; }
}