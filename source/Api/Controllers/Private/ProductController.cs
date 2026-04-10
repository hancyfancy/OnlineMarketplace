using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Asp.Versioning;
using Demo.Application.Catalog.Abstractions;
using Demo.Application.Catalog.Queries;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Api.Controllers.Private
{
    [Route("private/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Seller,Buyer")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] string? category,
            [FromQuery][Range(0, 1_000_000)] decimal? minPrice,
            [FromQuery][Range(0, 1_000_000)] decimal? maxPrice,
            [FromQuery] string? condition,
            [FromQuery] string sortBy = "relevance",
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int pageSize = 24)
        {
            Guid? sellerGuid = null;
            if (User.IsInRole("Seller") && Guid.TryParse(User.FindFirstValue("identity_guid"), out var guid))
                sellerGuid = guid;

            var query = new ProductSearchQuery(q, category, minPrice, maxPrice, condition, sortBy, page, pageSize, sellerGuid);
            var result = await _productService.SearchProductsAsync(query);

            if (!result.IsSuccess)
                return BadRequest(new ErrorResponse(result.Error.Code, result.Error.Description));

            return Ok(result.Value);
        }
    }
}
