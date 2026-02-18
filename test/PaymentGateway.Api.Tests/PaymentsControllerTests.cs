using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NSubstitute;

using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Integration.Bank.Models;
using PaymentGateway.Api.Models.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Tests.Helpers;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        (
            id: Guid.NewGuid(),
            expiryYear: _random.Next(2023, 2030),
            expiryMonth: _random.Next(1, 12),
            amount: _random.Next(1, 10000),
            cardNumberLastFour: _random.Next(1111, 9999),
            currency: "GBP",
            status: PaymentStatus.Authorized
        );

        var paymentsRepository = new InMemoryPaymentsRepository();
        await paymentsRepository.AddAsync(payment);

        var client = FactoryHelper.CreateFactory(null, paymentsRepository).CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var client = FactoryHelper.CreateFactory().CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task PostPayment_Returns200_WhenAuthorized()
    {
        // Arrange
        var request = new PostPaymentRequest
        (
            CardNumber: "4111111111111111",
            ExpiryMonth: 12,
            ExpiryYear: 2030,
            Currency: "GBP",
            Amount: 100,
            Cvv: 123
        );
        
        var bankClient = Substitute.For<IBankClient>();
        bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(BankPaymentResponse.CreateAuthorized("0bb07405-6d44-4b50-a14f-7ae0beff13ad"));
        var factory = FactoryHelper.CreateFactory(bankClient);
        var client = factory.CreateClient();
        

        // Act
        var resp = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(body);
        Assert.Equal(PaymentStatus.Authorized, body!.Status);
        await bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Received(1);
    }
    
    [Fact]
    public async Task PostPayment_Returns200_WhenDeclined()
    {
        // Arrange
        var request = new PostPaymentRequest(
            CardNumber: "2222405343248877",
            ExpiryMonth: 12,
            ExpiryYear: 2059,
            Currency: "GBP",
            Amount: 100,
            Cvv: 123);
        
        var bankClient = Substitute.For<IBankClient>();
        bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(BankPaymentResponse.CreateDeclined());
        var factory = FactoryHelper.CreateFactory(bankClient);
        var client = factory.CreateClient();
        
        // Act
        var resp = await client.PostAsJsonAsync("/payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.Equal(PaymentStatus.Declined, body!.Status);
        await bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>()).Received(1);
    }

    [Fact]
    public async Task PostPayment_Returns5xx_WhenBankFails()
    {
        /*var factory = FactoryHelper.CreateFactory();
        var client = factory.CreateClient();

        var request = ValidRequest();
        var resp = await client.PostAsJsonAsync("/payments", request);

        Assert.True((int)resp.StatusCode >= 500);
        Assert.Equal(1, fakeBank.Calls);*/
    }

}