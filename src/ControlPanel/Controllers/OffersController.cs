using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Offers.Commands;
using DeliverySystem.Application.Features.Products.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

public class OffersController(IMediator mediator) : Controller
{
    private static readonly List<(OfferType Type, string Label)> OfferTypes =
    [
        (OfferType.BuyOneGetOne,       "اشتر 1 واحصل على 1 مجاناً"),
        (OfferType.BuyOneGetTwo,       "اشتر 1 واحصل على 2 مجاناً"),
        (OfferType.BuyOneGetThree,     "اشتر 1 واحصل على 3 مجاناً"),
        (OfferType.DiscountPercentage, "خصم بالنسبة المئوية"),
        (OfferType.FixedPrice,         "سعر ثابت"),
        (OfferType.CustomVariable,     "عرض مخصص (قيمة مدخلة)")
    ];

    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var offers = await mediator.Send(new GetAllOffersQuery(search, isActive));
        ViewBag.Search   = search;
        ViewBag.IsActive = isActive;
        return View(offers);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Products   = await mediator.Send(new GetAllProductsQuery());
        ViewBag.OfferTypes = OfferTypes;
        return View(new CreateOfferDto { IsActive = true });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOfferDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products   = await mediator.Send(new GetAllProductsQuery());
            ViewBag.OfferTypes = OfferTypes;
            return View(dto);
        }
        await mediator.Send(new CreateOfferCommand(dto));
        TempData["Success"] = "تم إضافة العرض بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var offer = await mediator.Send(new GetOfferByIdQuery(id));
        if (offer is null) return NotFound();
        ViewBag.OfferId    = id;
        ViewBag.Products   = await mediator.Send(new GetAllProductsQuery());
        ViewBag.OfferTypes = OfferTypes;
        return View(new UpdateOfferDto
        {
            Name = offer.Name, Description = offer.Description,
            ProductId = offer.ProductId, OfferType = offer.OfferType,
            DiscountValue = offer.DiscountValue, FreeQuantity = offer.FreeQuantity,
            MinimumQuantity = offer.MinimumQuantity, PromoCode = offer.PromoCode,
            IsActive = offer.IsActive, StartDate = offer.StartDate, EndDate = offer.EndDate
        });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateOfferDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.OfferId    = id;
            ViewBag.Products   = await mediator.Send(new GetAllProductsQuery());
            ViewBag.OfferTypes = OfferTypes;
            return View(dto);
        }
        var ok = await mediator.Send(new UpdateOfferCommand(id, dto));
        if (!ok) return NotFound();
        TempData["Success"] = "تم تحديث العرض بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await mediator.Send(new DeleteOfferCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "تم حذف العرض بنجاح" : "حدث خطأ أثناء الحذف";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var offer = await mediator.Send(new GetOfferByIdQuery(id));
        if (offer is null) return NotFound();
        await mediator.Send(new UpdateOfferCommand(id, new UpdateOfferDto
        {
            Name = offer.Name, Description = offer.Description, ProductId = offer.ProductId,
            OfferType = offer.OfferType, DiscountValue = offer.DiscountValue,
            FreeQuantity = offer.FreeQuantity, MinimumQuantity = offer.MinimumQuantity,
            PromoCode = offer.PromoCode, IsActive = !offer.IsActive,
            StartDate = offer.StartDate, EndDate = offer.EndDate
        }));
        TempData["Success"] = "تم تغيير حالة العرض";
        return RedirectToAction(nameof(Index));
    }
}
