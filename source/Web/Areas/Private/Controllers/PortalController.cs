using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Private.Controllers
{
    [Area("Private")]
    [Authorize(Roles = "Seller,Buyer")]
    public class PortalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
