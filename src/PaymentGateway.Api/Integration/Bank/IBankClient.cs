using PaymentGateway.Api.Integration.Bank.Models;

namespace PaymentGateway.Api.Integration.Bank;

public interface IBankClient
{
    Task<BankPaymentResponse> ProcessPaymentAsync(BankPaymentRequest request,
        CancellationToken cancellationToken = default);
}