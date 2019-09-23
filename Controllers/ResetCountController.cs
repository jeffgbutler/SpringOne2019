using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;

namespace PaymentService.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ResetCountController
    {
        private IHitCountService HitCountService;

        public ResetCountController(IHitCountService hitCountService)
        {
            HitCountService = hitCountService;
        }

        [HttpGet]
        public ActionResult<string> ResetCount() {
            HitCountService.Reset();
            return "OK";
        }
    }
}
