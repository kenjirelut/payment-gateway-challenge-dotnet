using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Helpers;
using PaymentGateway.Api.Infrastructure;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentsRepository paymentsRepository , IPaymentService paymentService)
    {
        _paymentsRepository = paymentsRepository;
        _paymentService = paymentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse>> GetPaymentAsync(Guid id)
    {
        var paymentResult = await _paymentsRepository.GetAsync(id);

        return paymentResult.IsSuccess ? 
            Ok(paymentResult.Value!.MapToGetPaymentResponse())
            : NotFound(paymentResult.Error?.Description);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse?>> PostPaymentAsync(PostPaymentRequest request)
    {
        var response = await _paymentService.PostPaymentAsync(request);
        if (response.IsSuccess)
            return Ok(response.Value);

        var res = response.Error?.ErrorType switch
        {
            ErrorType.Internal => Problem(response.Error.Description),
            ErrorType.SubServiceUnavailable => new ObjectResult(new ProblemDetails{Detail = response.Error.Description, Status = 503}),
            ErrorType.Validation => BadRequest(response.Error.Description),
            ErrorType.NotFound => NotFound(response.Error.Description),
            _ => Problem(response.Error?.Description)
        };
        
        return res;
    }
}