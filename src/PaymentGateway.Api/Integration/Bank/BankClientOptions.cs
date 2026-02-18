using Microsoft.Extensions.Options;

namespace PaymentGateway.Api.Integration.Bank;

public class BankClientOptions
{
    public const string SectionName = "BankClientOptions";
    public string Url { get; init; } = string.Empty;
}