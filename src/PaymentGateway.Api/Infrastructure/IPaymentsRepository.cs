using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Infrastructure;

public interface IPaymentsRepository
{
    public Task<Result> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    public Task<Result<Payment>> GetAsync(Guid id, CancellationToken cancellationToken = default);
}