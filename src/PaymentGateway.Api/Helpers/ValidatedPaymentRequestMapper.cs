using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Integration.Bank.Models;
using PaymentGateway.Api.Models.Enums;

namespace PaymentGateway.Api.Helpers;

public static class ValidatedPaymentRequestMapper
{
    public static BankPaymentRequest MapToBankRequest(this ValidatedPaymentRequest v) =>
        new()
        {
            CardNumber = v.CardNumber,
            ExpiryDate = v.ExpiryDateForBank,
            Currency = v.Currency.ToString(), // ISO code
            Amount = v.Amount,
            Cvv = v.Cvv
        };

    public static Payment MapToPayment(this ValidatedPaymentRequest v, BankPaymentResponse bankPaymentResponse) =>
        new(
            Id: Guid.NewGuid(),
            Amount: v.Amount,
            CardNumberLastFour: v.CardNumberLastFour,
            Currency: v.Currency,
            ExpiryMonth: v.ExpiryMonth,
            ExpiryYear: v.ExpiryYear,
            Status: bankPaymentResponse.Authorized == true ? PaymentStatus.Authorized : PaymentStatus.Declined
        );
}