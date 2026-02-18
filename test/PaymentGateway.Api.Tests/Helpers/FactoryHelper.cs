using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Helpers;

public static class FactoryHelper
{
    public static WebApplicationFactory<Program> CreateFactory(
        IBankClient? bankClientSubstitute = null,
        IPaymentsRepository? repository = null)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    if (repository is not null)
                    {
                        services.RemoveAll<IPaymentsRepository>();
                        services.AddSingleton(repository);
                    }

                    if (bankClientSubstitute is not null)
                    {
                        services.RemoveAll<IBankClient>();
                        services.AddSingleton(bankClientSubstitute);
                    }
                });
            });
    }

}