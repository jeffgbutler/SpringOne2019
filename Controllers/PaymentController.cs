using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Models;
using PaymentService.Services;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace PaymentService.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class PaymentController
    {
        private PaymentCalculator PaymentCalculator;
        private CloudFoundryApplicationOptions AppOptions;
        private IHitCountService HitCountService;

        private readonly ILogger _logger;
        
        public PaymentController(PaymentCalculator paymentCalculator,
                IOptions<CloudFoundryApplicationOptions> appOptions,
                IHitCountService hitCountService,
                ILogger<PaymentController> logger)
        {
            PaymentCalculator = paymentCalculator;
            AppOptions = appOptions.Value;
            HitCountService = hitCountService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<CalculatedPayment> calculatePayment(double Amount, double Rate, int Years) {
            var Payment = PaymentCalculator.Calculate(Amount, Rate, Years);

            _logger.LogDebug("Calculated payment of {Payment} for input amount: {Amount}, rate: {Rate}, years: {Years}",
                Payment, Amount, Rate, Years);

            return new CalculatedPayment
            {
                Amount = Amount,
                Rate = Rate,
                Years = Years,
                Instance = AppOptions.InstanceIndex.ToString(),
                Count = HitCountService.GetAndIncrement(),
                Payment = Payment
            };
        }
    }
}
