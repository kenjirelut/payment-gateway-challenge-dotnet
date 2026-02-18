using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Integration.Bank;
using PaymentGateway.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bank client configuration
builder.Services
    .AddOptions<BankClientOptions>()
    .Bind(builder.Configuration.GetSection(BankClientOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Url), "Bank Url must be provided")
    .ValidateOnStart();
builder.Services.AddHttpClient<IBankClient, BankClient>();

// Service registrations
builder.Services.AddSingleton<IPaymentsRepository, InMemoryPaymentsRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// For test purposes
public partial class Program;
