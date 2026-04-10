using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

[Microsoft.AspNetCore.Authorization.AllowAnonymous]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public void OnGet()
    {
        RequestId = HttpContext.TraceIdentifier;
    }
}
