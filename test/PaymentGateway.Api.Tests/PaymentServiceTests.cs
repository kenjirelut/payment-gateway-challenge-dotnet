using NSubstitute;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Integration.Bank.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentServiceTests
{
    private static PostPaymentRequest ValidRequest() => new PostPaymentRequest(
        CardNumber: "4111111111111111",
        ExpiryMonth: 12,
        ExpiryYear: DateTime.UtcNow.Year + 1,
        Currency: "EUR",
        Amount: 100,
        Cvv: "123"
    );
    
    [Theory]
    [InlineData(true)] // authorized
    [InlineData(false)] // Declined
    public async Task PostPayment_ReturnsExpectedAuthorization_AndPersists(bool isAuthorized)
    {
        // Arrange
        var bank = Substitute.For<IBankClient>();
        var repo = Substitute.For<IPaymentsRepository>();

        bank.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new BankPaymentResponse { Authorized = isAuthorized, AuthorizationCode =  isAuthorized ? "AUTH123" : string.Empty });

        repo.AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new PaymentService(bank, repo);

        var expectedStatus = isAuthorized ? "Authorized" : "Declined";
        
        // Act
        var result = await sut.PostPaymentAsync(ValidRequest());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedStatus, result.Value!.Status);

        await bank.Received(1).ProcessPaymentAsync(
            Arg.Any<BankPaymentRequest>(),
            Arg.Any<CancellationToken>());

        await repo.Received(1).AddAsync(
            Arg.Any<Payment>(),
            Arg.Any<CancellationToken>());
    }

    
    [Fact]
    public async Task PostPayment_ReturnsValidationError_AndDoesNotCallBank_WhenInvalid()
    {
        // Arrange
        var bank = Substitute.For<IBankClient>();
        var repo = Substitute.For<IPaymentsRepository>();

        var sut = new PaymentService(bank, repo);

        var invalid = ValidRequest() with { CardNumber = "abc" };

        // Act
        var result = await sut.PostPaymentAsync(invalid);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);

        await bank.DidNotReceive().ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
        await repo.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task PostPayment_ReturnsSubServiceUnavailable_WhenBankUnavailable_AndDoesNotPersist()
    {
        // Arrange
        var bank = Substitute.For<IBankClient>();
        var repo = Substitute.For<IPaymentsRepository>();

        bank.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationError.BankError());

        var sut = new PaymentService(bank, repo);

        // Act
        var result = await sut.PostPaymentAsync(ValidRequest());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.SubServiceUnavailable, result.Error!.ErrorType);

        await repo.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task PostPayment_ReturnsInternalError_WhenPersistenceFails()
    {
        // Arrange
        var expectedErrorMessage = "Failed to persist payment";
        
        var bank = Substitute.For<IBankClient>();
        var repo = Substitute.For<IPaymentsRepository>();
        
        bank.ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new BankPaymentResponse { Authorized = true, AuthorizationCode = "AUTH123" });

        repo.AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>())
            .Returns(ApplicationError.Internal(expectedErrorMessage));
        
        var sut = new PaymentService(bank, repo);

        // Act
        var result = await sut.PostPaymentAsync(ValidRequest());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Internal, result.Error!.ErrorType);
        Assert.Equal(expectedErrorMessage, result.Error!.Description);

        await bank.Received(1).ProcessPaymentAsync(Arg.Any<BankPaymentRequest>(), Arg.Any<CancellationToken>());
        await repo.Received(1).AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }
    
}