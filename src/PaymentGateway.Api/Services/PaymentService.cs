using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly IBankClient _bankClient;
    private readonly IPaymentsRepository _paymentsRepository;

    // Support 3 currencies for simplicity
    private static readonly HashSet<IsoCurrency> SupportedCurrency =
    [
        IsoCurrency.EUR,
        IsoCurrency.USD,
        IsoCurrency.GBP
    ];
    public PaymentService(IBankClient bankClient, IPaymentsRepository paymentsRepository)
    {
        _bankClient = bankClient;
        _paymentsRepository = paymentsRepository;
    }

    public async Task<Result<PostPaymentResponse>> PostPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Parameter validation
        var requestValidatorResult = await ValidatePostPaymentRequestAsync(request, cancellationToken);
        if (!requestValidatorResult.IsSuccess)
            return requestValidatorResult.Error!;

        var validatedPaymentrequest = requestValidatorResult.Value!;
        // Bank processing
        var bankPaymentRequest = validatedPaymentrequest.MapToBankRequest();
        var bankResult = await _bankClient.ProcessPaymentAsync(bankPaymentRequest, cancellationToken);
        
        // Response handling
        if (!bankResult.IsSuccess)
        {
            return bankResult.Error!;
        }
        
        var payment = validatedPaymentrequest.MapToPayment(bankResult.Value!);
        var savePaymentResult = await _paymentsRepository.AddAsync(payment, cancellationToken);
        
        if (!savePaymentResult.IsSuccess)
            return savePaymentResult.Error!;
        
        return payment.MapToPostPaymentResponse();
    }

    public Task<Result<ValidatedPaymentRequest>> ValidatePostPaymentRequestAsync(PostPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        
        // Check card number
        string card = string.Empty;
        if (string.IsNullOrWhiteSpace(request.CardNumber))
        {
            errors.Add("CardNumber is required.");
        }
        else
        {
            card = request.CardNumber.Trim();

            if (!card.All(char.IsDigit))
                errors.Add("CardNumber must contain digits only.");

            if (card.Length < 14 || card.Length > 19)
                errors.Add("CardNumber length must be between 14 and 19 digits.");
        }
        
        // Check Expiry date
        var currentDate = DateTime.UtcNow;

        if (request.ExpiryMonth is < 1 or > 12)
        {
            errors.Add("ExpiryMonth must be between 1 and 12.");
        }
        else if (request.ExpiryYear is < 1000 or > 9999)
        {
            errors.Add("ExpiryYear must be a 4-digit year.");
        }
        else
        {
            var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1);
            var currentMonthYear = new DateTime(currentDate.Year, currentDate.Month, 1);
            if (currentMonthYear > expiryDate)
            {
                errors.Add("Card is expired.");
            }
        }
        
        // Check currency
        IsoCurrency currencyCode = default;
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add("Currency is required.");
        }
        else
        {
            string currency = request.Currency.Trim().ToUpperInvariant();

            if (!Enum.TryParse(currency, ignoreCase: true, out currencyCode))
            {
                errors.Add("Currency must be a valid 3-letter ISO 4217 code. ");
            }
            else if (!SupportedCurrency.Contains(currencyCode))
            {
                errors.Add("Currency is not supported. ");
            }
        }
        
        // Check amount 
        if (request.Amount <= 0)
            errors.Add("Amount must be greater than 0. ");
        
        // Check CVV
        string cvv = string.Empty;
        if (string.IsNullOrWhiteSpace(request.Cvv))
        {
            errors.Add("Cvv is required. ");
        }
        else
        {
            cvv = request.Cvv.Trim();
            if (!cvv.All(char.IsDigit) || cvv.Length is not (4 or 3))
                errors.Add("Cvv is invalid: must be a 3 or 4 digits long sequence. ");
        }

        if (errors.Count > 0)
        {
            return Task.FromResult(
                (Result<ValidatedPaymentRequest>)new ApplicationError(ErrorType.Validation, string.Join(" ", errors).Trim()));
        }
            
        var validatedPaymentRequest = new ValidatedPaymentRequest(
            CardNumber: card,
            CardNumberLastFour: card[^4..],
            ExpiryMonth: request.ExpiryMonth,
            ExpiryYear: request.ExpiryYear,
            Currency: currencyCode,
            Amount: request.Amount,
            Cvv: cvv);
        return Task.FromResult((Result<ValidatedPaymentRequest>)validatedPaymentRequest);
        

    }
}