using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Api.Models;
using Asp.Versioning;
using System.ComponentModel.DataAnnotations;
using Demo.Application.Security.Abstractions;
using Demo.Application.Security.Queries;

namespace Api.Controllers.Identity
{
    [Route("identity/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class TokenController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public TokenController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public record LoginRequest(
            [Required][EmailAddress] string Email,
            [Required][MinLength(1)] string ApiKey);

        [HttpPost("login")]
        [EnableRateLimiting("token")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.IssueTokenAsync(new IssueTokenQuery(request.Email, request.ApiKey));

            if (!result.IsSuccess)
                return Unauthorized(new ErrorResponse(result.Error.Code, result.Error.Description));

            return Ok(new { result.Value.AccessToken, result.Value.ExpiresAt });
        }
    }
}
