using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Integration.Bank.Models;

namespace PaymentGateway.Api.Integration.Bank;

public interface IBankClient
{
    Task<Result<BankPaymentResponse>> ProcessPaymentAsync(BankPaymentRequest request,
        CancellationToken cancellationToken = default);
}