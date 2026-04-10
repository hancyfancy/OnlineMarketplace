using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    [AllowAnonymous]
    public class AnonymousController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [EnableRateLimiting("login")]
        public IActionResult Login(Login model)
        {
            var properties = new AuthenticationProperties { RedirectUri = "/Private/Portal/Index" };

            return Challenge(properties, "Google");
        }
    }
}
