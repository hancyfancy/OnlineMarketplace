using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Application.Catalog.Abstractions;
using Demo.Application.Catalog.Commands;
using Demo.Application.Catalog.Dtos;
using Demo.Application.Catalog.Queries;
using System.Security.Claims;
using Web.Areas.Private.Models;

namespace Web.Areas.Private.Controllers
{
    [Area("Private")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IAuthorizationService _authorizationService;

        public ProductController(IProductService productService, IAuthorizationService authorizationService)
        {
            _productService = productService;
            _authorizationService = authorizationService;
        }

        private async Task<List<SelectListItem>> GetCategoryOptionsAsync(Guid selectedId = default)
        {
            var result = await _productService.GetCategoriesAsync(new GetCategoriesQuery());
            if (!result.IsSuccess) return [];

            return result.Value.Select(c => new SelectListItem
            {
                Value = c.CategoryGuid.ToString(),
                Text = c.Name,
                Selected = c.CategoryGuid == selectedId
            }).OrderBy(c => c.Text).ToList();
        }

        [HttpGet]
        [Authorize(Roles = "Seller,Buyer")]
        public async Task<IActionResult> Search(
            string? q,
            string? category,
            decimal? minPrice,
            decimal? maxPrice,
            string? condition,
            string sortBy = "relevance",
            string viewMode = "grid",
            int page = 1)
        {
            var isActiveSeller = (await _authorizationService.AuthorizeAsync(User, null, "ActiveSeller")).Succeeded;
            Guid? sellerGuid = isActiveSeller
                ? Guid.Parse(User.FindFirstValue("identity_guid")!)
                : null;

            var query = new ProductSearchQuery(q, category, minPrice, maxPrice, condition, sortBy, page, 24, sellerGuid);
            var result = await _productService.SearchProductsAsync(query);

            var model = new ProductSearchModel
            {
                Q = q,
                Category = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Condition = condition,
                SortBy = sortBy,
                ViewMode = viewMode,
                Page = page
            };

            if (result.IsSuccess)
            {
                model.TotalCount = result.Value.TotalCount;
                model.TotalPages = result.Value.TotalPages;
                model.Items = result.Value.Items.Select(dto => new ProductSearchItemModel
                {
                    ProductGuid = dto.ProductGuid,
                    Slug = dto.Slug,
                    Name = dto.Name,
                    Brand = dto.Brand,
                    ShortDescription = dto.ShortDescription,
                    EffectivePrice = dto.IsOnSale && dto.SalePrice.HasValue ? dto.SalePrice.Value
                                       : dto.VariantMinPrice ?? dto.BasePrice ?? 0m,
                    OriginalPrice = dto.IsOnSale && dto.SalePrice.HasValue ? dto.BasePrice : null,
                    IsOnSale = dto.IsOnSale,
                    DiscountPercentage = dto.DiscountPercentage,
                    AverageRating = dto.AverageRating,
                    ReviewCount = dto.ReviewCount,
                    SoldCount = dto.SoldCount,
                    IsInStock = dto.IsInStock,
                    IsLowStock = dto.IsLowStock,
                    IsFeatured = dto.IsFeatured,
                    IsDigital = dto.IsDigital,
                    ShippingType = dto.ShippingType,
                    ShippingCost = dto.ShippingCost,
                    ThumbnailUrl = dto.ThumbnailUrl,
                    Condition = dto.Condition,
                    CategoryName = dto.CategoryName,
                    SellerGuid = dto.SellerGuid,
                    IsArchived = dto.IsArchived
                }).ToList();
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "ActiveSeller")]
        public async Task<IActionResult> Add()
        {
            var model = new AddProductModel
            {
                CategoryOptions = await GetCategoryOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "ActiveSeller")]
        public async Task<IActionResult> Add(AddProductModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryOptions = await GetCategoryOptionsAsync(model.CategoryGuid);
                return View(model);
            }

            var sellerGuid = Guid.Parse(User.FindFirstValue("identity_guid")!);

            var dto = new AddProductDto
            {
                CategoryGuid = model.CategoryGuid,
                Name = model.Name,
                ShortDescription = model.ShortDescription,
                Brand = model.Brand,
                SearchKeywords = model.SearchKeywords,
                IsDigital = model.IsDigital,
                ShippingType = model.ShippingType,
                ShippingCost = model.ShippingCost,
                BasePrice = model.BasePrice,
                Variants = model.Variants.Select((v, i) => new AddProductVariantDto
                {
                    Name = v.Name,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material,
                    Condition = v.Condition,
                    UnitPrice = v.UnitPrice,
                    CompareAtPrice = v.CompareAtPrice,
                    UnitsInStock = v.UnitsInStock,
                    UnitsReserved = v.UnitsReserved,
                    ImageUrl = v.ImageUrl,
                    SortIndex = i
                }).ToList()
            };

            var result = await _productService.AddProductAsync(new AddProductCommand(sellerGuid, dto));

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error.Description);
                model.CategoryOptions = await GetCategoryOptionsAsync(model.CategoryGuid);
                return View(model);
            }

            return RedirectToAction(nameof(Search));
        }

        [HttpGet]
        [Authorize(Policy = "ActiveSeller")]
        public async Task<IActionResult> Update(Guid productGuid)
        {
            var result = await _productService.GetProductAsync(new GetProductQuery(productGuid));

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Error.Code == "ProductNotFound"
                    ? "Product not found or has been archived."
                    : result.Error.Description;
                return RedirectToAction(nameof(Search));
            }

            var dto = result.Value;
            var sellerGuid = Guid.Parse(User.FindFirstValue("identity_guid")!);

            if (dto.SellerGuid != sellerGuid)
            {
                TempData["Error"] = "You do not have permission to edit this product.";
                return RedirectToAction(nameof(Search));
            }

            var model = new UpdateProductModel
            {
                ProductGuid = dto.ProductGuid,
                SellerGuid = dto.SellerGuid,
                Name = dto.Name,
                CategoryGuid = dto.CategoryGuid,
                ShortDescription = dto.ShortDescription,
                Brand = dto.Brand,
                SearchKeywords = dto.SearchKeywords,
                IsDigital = dto.IsDigital,
                ShippingType = dto.ShippingType,
                ShippingCost = dto.ShippingCost,
                BasePrice = dto.BasePrice,
                IsActive = dto.IsActive,
                CategoryOptions = await GetCategoryOptionsAsync(dto.CategoryGuid),
                Variants = dto.Variants.Select(v => new UpdateVariantModel
                {
                    VariantGuid = v.VariantGuid,
                    Name = v.Name,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material,
                    Condition = v.Condition ?? "new",
                    UnitPrice = v.UnitPrice,
                    CompareAtPrice = v.CompareAtPrice,
                    UnitsInStock = v.UnitsInStock,
                    UnitsReserved = v.UnitsReserved,
                    ImageUrl = v.ImageUrl,
                    SortIndex = v.SortIndex
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "ActiveSeller")]
        public async Task<IActionResult> Update(UpdateProductModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryOptions = await GetCategoryOptionsAsync(model.CategoryGuid);
                return View(model);
            }

            var sellerGuid = Guid.Parse(User.FindFirstValue("identity_guid")!);

            if (model.SellerGuid != Guid.Empty && model.SellerGuid != sellerGuid)
            {
                TempData["Error"] = "You do not have permission to edit this product.";
                return RedirectToAction(nameof(Search));
            }

            var dto = new UpdateProductDto
            {
                ProductGuid = model.ProductGuid,
                CategoryGuid = model.CategoryGuid,
                Name = model.Name,
                ShortDescription = model.ShortDescription,
                Brand = model.Brand,
                SearchKeywords = model.SearchKeywords,
                IsDigital = model.IsDigital,
                ShippingType = model.ShippingType,
                ShippingCost = model.ShippingCost,
                BasePrice = model.BasePrice,
                IsActive = model.IsActive,
                Variants = model.Variants.Select((v, i) => new UpdateProductVariantDto
                {
                    VariantGuid = v.VariantGuid,
                    Name = v.Name,
                    Color = v.Color,
                    Size = v.Size,
                    Material = v.Material,
                    Condition = v.Condition,
                    UnitPrice = v.UnitPrice,
                    CompareAtPrice = v.CompareAtPrice,
                    UnitsInStock = v.UnitsInStock,
                    UnitsReserved = v.UnitsReserved,
                    ImageUrl = v.ImageUrl,
                    SortIndex = i
                }).ToList()
            };

            var result = await _productService.UpdateProductAsync(new UpdateProductCommand(sellerGuid, dto));

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error.Description);
                model.CategoryOptions = await GetCategoryOptionsAsync(model.CategoryGuid);
                return View(model);
            }

            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Search));
        }

        [HttpPost]
        [Authorize(Policy = "ActiveSeller")]
        public async Task<IActionResult> Archive(Guid productGuid)
        {
            var sellerGuid = Guid.Parse(User.FindFirstValue("identity_guid")!);
            var result = await _productService.ArchiveProductAsync(new ArchiveProductCommand(productGuid, sellerGuid));

            if (!result.IsSuccess)
                TempData["Error"] = result.Error.Description;
            else
                TempData["Success"] = "Product archived successfully.";

            return RedirectToAction(nameof(Search));
        }
    }
}
