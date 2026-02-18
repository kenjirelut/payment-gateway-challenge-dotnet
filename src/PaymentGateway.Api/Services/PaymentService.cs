using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly IBankClient _bankClient;

    public PaymentService(IBankClient bankClient)
    {
        _bankClient = bankClient;
    }

    public Task<Result<PostPaymentResponse>> PostPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Parameter validation
        
        // Bank processing
        
        // Response handling
        throw new NotImplementedException();
    }
}