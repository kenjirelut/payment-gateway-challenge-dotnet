using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Helpers;
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
    public async Task GetPayment_RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        (
            id: Guid.NewGuid(),
            expiryYear: _random.Next(2023, 2030),
            expiryMonth: _random.Next(1, 13), // 13 is excluded
            amount: _random.Next(1, 10000),
            cardNumberLastFour: _random.Next(1111, 9999).ToString(),
            currency: "GBP",
            status: nameof(PaymentStatus.Authorized)
        );

        var paymentsRepository = new InMemoryPaymentsRepository();
        await paymentsRepository.AddAsync(payment.Map());

        var client = FactoryHelper.CreateFactory(null, paymentsRepository).CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task GetPayment_Returns404_WhenPaymentNotFound()
    {
        // Arrange
        var client = FactoryHelper.CreateFactory().CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Theory]
    [InlineData(PaymentStatus.Authorized)]
    [InlineData(PaymentStatus.Declined)]
    public async Task PostPayment_Returns200_WhenAuthorizedOrDeclined(PaymentStatus expectedPaymentStatus)
    {
        // Arrange
        var request = new PostPaymentRequest
        (
            CardNumber: "4111111111111111",
            ExpiryMonth: 12,
            ExpiryYear: 2030,
            Currency: "GBP",
            Amount: 100,
            Cvv: "123"
        );
        
        var expectedBankPaymentResponse = expectedPaymentStatus == PaymentStatus.Authorized ?
            BankPaymentResponse.CreateAuthorized("0bb07405-6d44-4b50-a14f-7ae0beff13ad")
            : BankPaymentResponse.CreateDeclined();
        var bankClient = Substitute.For<IBankClient>();
        bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedBankPaymentResponse);
        var factory = FactoryHelper.CreateFactory(bankClient);
        var client = factory.CreateClient();
        

        // Act
        var resp = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(body);
        Assert.True(Enum.TryParse<PaymentStatus>(body!.Status, out var paymentStatus));
        Assert.Equal(expectedPaymentStatus, paymentStatus);
        await bankClient.Received(1)
                .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostPayment_Returns503_WhenBankUnavailable()
    {
        // Arrange
        var request = new PostPaymentRequest
        (
            CardNumber: "4111111111111111",
            ExpiryMonth: 12,
            ExpiryYear: 2030,
            Currency: "GBP",
            Amount: 100,
            Cvv: "123"
        );

        var bankError = ApplicationError.BankError();
        var bankClient = Substitute.For<IBankClient>();
        bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(bankError);
        var factory = FactoryHelper.CreateFactory(bankClient);
        var client = factory.CreateClient();
        

        // Act
        var resp = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);


        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(bankError.Description, problem!.Detail);
        await bankClient.Received(1)
            .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PostPayment_Returns500_WhenBankReturnsABadRequest()
    {
        // Arrange
        var request = new PostPaymentRequest
        (
            CardNumber: "4111111111111111",
            ExpiryMonth: 12,
            ExpiryYear: 2030,
            Currency: "GBP",
            Amount: 100,
            Cvv: "123"
        );

        var bankError = ApplicationError.BankBadRequest();
        var bankClient = Substitute.For<IBankClient>();
        bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(bankError);
        var factory = FactoryHelper.CreateFactory(bankClient);
        var client = factory.CreateClient();
        

        // Act
        var resp = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, resp.StatusCode);


        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(bankError.Description, problem!.Detail!);
        await bankClient.Received(1)
            .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task PostPayment_Returns400_WhenRequestIsInvalid()
    {
        // Arrange
        var request = new PostPaymentRequest
        (
            CardNumber: "41111111fdg11111111",
            ExpiryMonth: 12,
            ExpiryYear: 2030,
            Currency: "GBP",
            Amount: 100,
            Cvv: "123"
        );
        
        var bankClient = Substitute.For<IBankClient>();
        
        var factory = FactoryHelper.CreateFactory(bankClient);
        var client = factory.CreateClient();
        

        // Act
        var resp = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        await bankClient.DidNotReceive()
            .ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task PostThenGetAPayment_ReturnsPersistedPayment()
    {
        // Arrange
        var request = new PostPaymentRequest(
            "4111111111111111", 12, 2030, "GBP", 100, "123");

        var bankClient = Substitute.For<IBankClient>();
        bankClient.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(BankPaymentResponse.CreateAuthorized("AUTH123"));

        var repo = new InMemoryPaymentsRepository();
        var client = FactoryHelper.CreateFactory(bankClient, repo).CreateClient();

        // Act
        var post = await client.PostAsJsonAsync("/api/Payments", request);
        post.EnsureSuccessStatusCode();

        var postBody = await post.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(postBody);

        var get = await client.GetAsync($"/api/Payments/{postBody!.Id}");
        get.EnsureSuccessStatusCode();

        var getBody = await get.Content.ReadFromJsonAsync<GetPaymentResponse>();
        
        // Assert
        Assert.NotNull(getBody);
        Assert.Equal(postBody.Id, getBody!.Id);
        Assert.Equal(postBody.Status, getBody.Status);
        Assert.Equal(postBody.Currency, getBody.Currency);
    }
}