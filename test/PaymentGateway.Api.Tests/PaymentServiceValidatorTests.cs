using NSubstitute;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentServiceValidatorTests
{
    private static PaymentService CreateService()
    {
        var bank = Substitute.For<IBankClient>();
        var repo = Substitute.For<IPaymentsRepository>();
        return new PaymentService(bank, repo);
    }
    
    private static PostPaymentRequest ValidRequest() => new PostPaymentRequest(
        CardNumber: "4111111111111111",
        ExpiryMonth: 12,
        ExpiryYear: DateTime.UtcNow.Year + 1,
        Currency: "EUR",
        Amount: 100,
        Cvv: "123"
    );

    [Fact]
    public async Task Validate_ReturnsSuccess_WithValidatedModel_WhenRequestIsValid()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest();

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("4111111111111111", result.Value!.CardNumber);
        Assert.Equal("1111", result.Value.CardNumberLastFour);
        Assert.Equal(IsoCurrency.EUR, result.Value.Currency);
        Assert.Equal(100, result.Value.Amount);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("KJii2sdbi")] // non digit
    [InlineData(null)]
    [InlineData("12")] // too short
    [InlineData("41111111111111110000000")] // too long
    public async Task Validate_Fails_WhenCardNumberMissing(string cardNumber)
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { CardNumber = cardNumber };

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("CardNumber", result.Error!.Description);
    }
    
    [Fact]
    public async Task Validate_Fails_WhenExpiryMonthInvalid()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { ExpiryMonth = 13 };

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("ExpiryMonth", result.Error!.Description);
    }

    [Fact]
    public async Task Validate_Fails_WhenCardExpired()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { ExpiryYear = DateTime.UtcNow.Year - 1 };

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("Card is expired", result.Error!.Description);
    }

    [Fact]
    public async Task Validate_Fails_WhenExpiryYearInvalid()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { ExpiryYear = 855};

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("ExpiryYear", result.Error!.Description);
    }
    
    [Fact]
    public async Task Validate_Fails_WhenCurrencyInvalidIso()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { Currency = "AAA" };

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("Currency", result.Error!.Description);
    }

    [Fact]
    public async Task Validate_Fails_WhenCurrencyNotSupported()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { Currency = "JPY" }; 

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("Currency is not supported", result.Error!.Description);
    }

    [Fact]
    public async Task Validate_Fails_WhenAmountNonPositive()
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { Amount = -2 };

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("Amount", result.Error!.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12a")]
    public async Task Validate_Fails_WhenCvvInvalid(string cvv)
    {
        // Arrange
        var sut = CreateService();
        var req = ValidRequest() with { Cvv = cvv };

        // Act
        var result = await sut.ValidatePostPaymentRequestAsync(req);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Contains("Cvv", result.Error!.Description);
    }
}