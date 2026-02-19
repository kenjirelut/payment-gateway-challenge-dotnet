using System.Collections.Concurrent;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructure;

public class InMemoryPaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid,Payment> _payments = new();
    
    public Task<Result> AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var succeed= _payments.TryAdd(payment.Id, payment);
        var result = succeed
            ? Result.Success()
            : Result.Failure(ApplicationError.Internal($"Failed to persist payment {payment.Id}"));
        return Task.FromResult(result);
    }

    public Task<Result<Payment>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var success = _payments.TryGetValue(id, out var payment);
        return !success ? 
            Task.FromResult((Result<Payment>)ApplicationError.PaymentNotFound()) 
            : Task.FromResult((Result<Payment>)payment!);
    } 
        
}