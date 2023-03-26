using PaymentMS.src.DTOs;
using PaymentMS.src.Entities;
using PaymentMS.src.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Stripe;

namespace PaymentMS.src.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public PaymentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("CreatePayment")]
    public async Task<APIResponse> CreatePayment(Payment apiRequest)
    {
        try
        {
            Create(apiRequest);
            var data = await _unitOfWork.Payments.AddAsync(apiRequest);
            return new APIResponse
            {
                code = "200",
                message = "Success"
            };
        }
        catch(Exception ex)
        {
            return new APIResponse
            {
                code = "500",
                message = ex.ToString()
            };
        }
    }

    public object Create(Payment request)
    {
      var paymentIntentService = new PaymentIntentService();
      var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
      {
        Amount = (long)request.PaymentAmount,
        Currency = "usd",
        AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
        {
          Enabled = true,
        },
      });

      return new { clientSecret = paymentIntent.ClientSecret };
    }

}
