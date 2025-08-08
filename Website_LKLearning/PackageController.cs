using Microsoft.Extensions.Logging;

namespace WebLuanVan_ASP.NET_MVC.Areas.User.Controllers
{
    [Area("User")]
    public class PackageController : BaseController
    {
        private readonly ILogger<PackageController> _logger;

        public PackageController(ILogger<PackageController> logger)
        {
            _logger = logger;
        }

        [Route("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var status = Request.Query["status"].ToString();
            var appTransId = Request.Query["apptransid"].ToString();
            var orderId = HttpContext.Session.GetString("CurrentOrder") != null
                ? JsonConvert.DeserializeAnonymousType(HttpContext.Session.GetString("CurrentOrder"), new { OrderId = "" }).OrderId
                : "";

            _logger.LogInformation($"PaymentCallback: status={status}, appTransId={appTransId}, orderId={orderId}");

            // Rest of the method remains unchanged...
        }
    }
}
