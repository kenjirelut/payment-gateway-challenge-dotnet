using System.Collections.Concurrent;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructure;

public class InMemoryPaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid,PostPaymentResponse> _payments = new();
    
    public Task AddAsync(PostPaymentResponse payment, CancellationToken cancellationToken = default)
    {
        _payments.TryAdd(payment.Id, payment);
        return Task.CompletedTask;
    }

    public Task<PostPaymentResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var succeed = _payments.TryGetValue(id, out var payment);
        return Task.FromResult(payment);
    }
}