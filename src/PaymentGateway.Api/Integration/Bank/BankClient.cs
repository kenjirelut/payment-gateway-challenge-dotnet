using System.Net;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Integration.Bank.Models;

namespace PaymentGateway.Api.Integration.Bank;

public class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;
    private readonly BankClientOptions _options;

    public BankClient(HttpClient httpClient, IOptions<BankClientOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.Url);
    }

    public async Task<Result<BankPaymentResponse>> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/payments",
            request,
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.ServiceUnavailable => ApplicationError.BankError(),
                HttpStatusCode.BadRequest => ApplicationError.BankBadRequest(),
                _ => ApplicationError.BankError(response.ReasonPhrase)
            };
        }

        var paymentResponse = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken: cancellationToken);
        
        if (paymentResponse == null)
            return ApplicationError.BankError("Bank payment response is invalid");
        
        return paymentResponse.Authorized == true
            ? BankPaymentResponse.CreateAuthorized(paymentResponse.AuthorizationCode ?? string.Empty)
            : BankPaymentResponse.CreateDeclined();
    }
}