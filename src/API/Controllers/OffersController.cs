using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Offers.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

/// <summary>API للعروض والخصومات</summary>
[ApiController]
[Route("api/offers")]
[Produces("application/json")]
public class OffersController(IMediator mediator) : ControllerBase
{
    /// <summary>التحقق من العروض المتاحة لمنتج معين</summary>
    [HttpGet("check")]
    [Authorize]
    public async Task<IActionResult> CheckOffers([FromQuery] int? productId, [FromQuery] string? promoCode)
    {
        var result = await mediator.Send(new CheckProductOffersQuery(productId, promoCode));
        return Ok(ApiResponse<IEnumerable<OfferDto>>.Ok(result));
    }

    /// <summary>التحقق من صحة بروموكود</summary>
    [HttpGet("validate-promo")]
    [Authorize]
    public async Task<IActionResult> ValidatePromo([FromQuery] string promoCode, [FromQuery] int? productId)
    {
        if (string.IsNullOrWhiteSpace(promoCode))
            return BadRequest(ApiResponse<object>.Fail("كود البروموكود مطلوب", "Promo code is required"));
        var result = await mediator.Send(new CheckProductOffersQuery(productId, promoCode));
        var offers = result.ToList();
        if (!offers.Any())
            return NotFound(ApiResponse<object>.Fail("الكود غير صالح أو منتهي الصلاحية", "Invalid or expired promo code"));
        return Ok(ApiResponse<IEnumerable<OfferDto>>.Ok(offers, "الكود صالح", "Valid promo code"));
    }

    /// <summary>جلب جميع العروض النشطة</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetActive()
    {
        var result = await mediator.Send(new GetAllOffersQuery(IsActive: true));
        return Ok(ApiResponse<IEnumerable<OfferDto>>.Ok(result));
    }
}
