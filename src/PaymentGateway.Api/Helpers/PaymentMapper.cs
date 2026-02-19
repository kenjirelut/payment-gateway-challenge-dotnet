using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Helpers;

public static class PaymentMapper
{
    public static Payment Map(this PostPaymentResponse postPaymentResponse)
    {
        var paymentStatus = Enum.Parse<PaymentStatus>(postPaymentResponse.Status);
        var currencyCode = Enum.Parse<IsoCurrency>(postPaymentResponse.Currency);
        return new Payment(
            postPaymentResponse.Id,
            paymentStatus,
            postPaymentResponse.CardNumberLastFour,
            postPaymentResponse.ExpiryMonth,
            postPaymentResponse.ExpiryYear,
            currencyCode,
            postPaymentResponse.Amount);
    }

    public static PostPaymentResponse MapToPostPaymentResponse(this Payment payment)
    {
        return new PostPaymentResponse(
            payment.Id,
            payment.Status.ToString(),
            payment.CardNumberLastFour,
            payment.ExpiryMonth,
            payment.ExpiryYear,
            payment.Currency.ToString(),
            payment.Amount);
    }

    public static GetPaymentResponse MapToGetPaymentResponse(this Payment payment)
    {
        return new GetPaymentResponse(
            payment.Id,
            payment.Status.ToString(),
            payment.CardNumberLastFour,
            payment.ExpiryMonth,
            payment.ExpiryYear,
            payment.Currency.ToString(),
            payment.Amount);
    }
}