using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Integration.Bank.Models;

public class BankPaymentResponse
{
    [JsonPropertyName("authorized")]
    public bool? Authorized { get; init; }
    
    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; init; }
    
    public static BankPaymentResponse CreateAuthorized(string authorizationCode)
    {
        return new BankPaymentResponse { Authorized = true, AuthorizationCode = authorizationCode };
    }

    public static BankPaymentResponse CreateDeclined()
    {
        return new  BankPaymentResponse { Authorized = false };
    }
}