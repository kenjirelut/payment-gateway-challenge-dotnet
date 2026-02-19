using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Domain;

public record Payment(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    IsoCurrency Currency,
    int Amount);