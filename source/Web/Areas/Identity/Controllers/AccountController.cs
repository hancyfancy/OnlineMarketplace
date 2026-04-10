using Demo.Application.Security.Abstractions;
using Demo.Application.Security.Commands;
using Demo.Application.Security.Dtos;
using Demo.Application.Security.Queries;
using Demo.Domain.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Areas.Identity.Models;

namespace Web.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email)!;
            Result<UserProfileDto> result = await _accountService.GetUserProfileAsync(new UserProfileQuery(email));

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error.Description);
                return View(new Profile());
            }

            var identityGuid = Guid.Parse(User.FindFirstValue("identity_guid")!);
            var sellerResult = await _accountService.GetSellerProfileAsync(new SellerProfileQuery(identityGuid));
            var buyerResult = await _accountService.GetBuyerProfileAsync(new BuyerProfileQuery(identityGuid));

            var model = new Profile
            {
                Id = result.Value.Id,
                Email = result.Value.Email,
                FirstName = result.Value.FirstName,
                LastName = result.Value.LastName,
                PhoneNumber = result.Value.PhoneNumber,
                Role = result.Value.Role,
                CreatedAt = result.Value.CreatedAt,
                LastLoginAt = result.Value.LastLoginAt
            };

            if (sellerResult.IsSuccess)
            {
                var s = sellerResult.Value;
                model.SellerProfile = new SellerProfile
                {
                    BusinessName = s.BusinessName,
                    BusinessType = s.BusinessType,
                    BusinessTaxId = s.BusinessTaxId,
                    BusinessRegistrationNumber = s.BusinessRegistrationNumber,
                    BusinessRegistrationCountry = s.BusinessRegistrationCountry,
                    VatNumber = s.VatNumber,
                    HandlingTimeDays = s.HandlingTimeDays,
                    ReturnPolicyDays = s.ReturnPolicyDays,
                    RefundPolicy = s.RefundPolicy,
                    PayoutSchedule = s.PayoutSchedule,
                    SellerTier = s.SellerTier,
                    SellerStatus = s.SellerStatus,
                };
            }

            if (buyerResult.IsSuccess)
            {
                var b = buyerResult.Value;
                model.BuyerProfile = new Web.Areas.Identity.Models.BuyerProfile
                {
                    PreferredCurrency = b.PreferredCurrency,
                    BuyerLevel = b.BuyerLevel,
                    TrustScore = b.TrustScore,
                    TotalPurchases = b.TotalPurchases,
                    AvgOrderValue = b.AvgOrderValue,
                    TotalReturns = b.TotalReturns,
                    OpenDisputes = b.OpenDisputes,
                    DisputeRate = b.DisputeRate,
                };
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Profile model)
        {
            var claimEmail = User.FindFirstValue(ClaimTypes.Email)!;
            model.Email = claimEmail;
            model.Role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (!ModelState.IsValid)
                return View(model);

            var profileDto = new UserProfileUpdateDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Role = string.Empty,
            };

            Result profileResult = await _accountService.UpdateUserProfileAsync(new UserProfileUpdateCommand(claimEmail, profileDto));

            if (!profileResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, profileResult.Error.Description);
                return View(model);
            }

            var sellerDto = new SellerProfileUpdateDto
            {
                BusinessName = model.SellerProfile.BusinessName,
                BusinessType = model.SellerProfile.BusinessType,
                BusinessTaxId = model.SellerProfile.BusinessTaxId,
                BusinessRegistrationNumber = model.SellerProfile.BusinessRegistrationNumber,
                BusinessRegistrationCountry = model.SellerProfile.BusinessRegistrationCountry,
                VatNumber = model.SellerProfile.VatNumber,
                HandlingTimeDays = model.SellerProfile.HandlingTimeDays,
                ReturnPolicyDays = model.SellerProfile.ReturnPolicyDays,
                RefundPolicy = model.SellerProfile.RefundPolicy,
                PayoutSchedule = model.SellerProfile.PayoutSchedule,
            };

            var identityGuid = Guid.Parse(User.FindFirstValue("identity_guid")!);
            Result sellerResult = await _accountService.UpdateSellerProfileAsync(new SellerProfileUpdateCommand(identityGuid, sellerDto));

            if (!sellerResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, sellerResult.Error.Description);
                return View(model);
            }

            var buyerDto = new BuyerProfileUpdateDto
            {
                PreferredCurrency = model.BuyerProfile.PreferredCurrency,
            };

            Result buyerResult = await _accountService.UpdateBuyerProfileAsync(new BuyerProfileUpdateCommand(identityGuid, buyerDto));

            if (!buyerResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, buyerResult.Error.Description);
                return View(model);
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult Api()
        {
            return View(new Api());
        }

        [HttpPost]
        public async Task<IActionResult> RegenerateApiKey()
        {
            var email = User.FindFirstValue(ClaimTypes.Email)!;
            var result = await _accountService.RegenerateApiKeyAsync(new RegenerateApiKeyCommand(email));

            if (!result.IsSuccess)
            {
                TempData["ApiKeyError"] = result.Error.Description;
                return RedirectToAction(nameof(Api));
            }

            TempData["NewApiKey"] = result.Value;
            return RedirectToAction(nameof(Api));
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login", "Anonymous", new { area = "Identity" });
        }
    }
}
