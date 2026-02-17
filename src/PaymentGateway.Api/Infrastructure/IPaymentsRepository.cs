using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructure;

public interface IPaymentsRepository
{
    public Task AddAsync(PostPaymentResponse payment, CancellationToken cancellationToken = default);
    public Task<PostPaymentResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}