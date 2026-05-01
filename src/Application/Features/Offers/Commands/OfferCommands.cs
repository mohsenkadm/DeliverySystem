using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Offers.Commands;

// ── Helper ───────────────────────────────────────────────────────────────────

public static class OfferMapper
{
    public static OfferDto Map(Offer o) => new()
    {
        Id = o.Id, Name = o.Name, Description = o.Description,
        ProductId = o.ProductId, ProductName = o.Product?.Name,
        OfferType = o.OfferType, DiscountValue = o.DiscountValue,
        FreeQuantity = o.FreeQuantity, MinimumQuantity = o.MinimumQuantity,
        PromoCode = o.PromoCode, IsActive = o.IsActive,
        StartDate = o.StartDate, EndDate = o.EndDate, CreatedAt = o.CreatedAt
    };
}

// ── Queries ───────────────────────────────────────────────────────────────────

public record GetAllOffersQuery(string? Search = null, bool? IsActive = null) : IRequest<IEnumerable<OfferDto>>;

public class GetAllOffersQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllOffersQuery, IEnumerable<OfferDto>>
{
    public async Task<IEnumerable<OfferDto>> Handle(GetAllOffersQuery request, CancellationToken ct)
    {
        IQueryable<Offer> q = uow.Offers.Query().Include(o => o.Product);
        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(o => o.Name.Contains(request.Search) || (o.PromoCode != null && o.PromoCode.Contains(request.Search)));
        if (request.IsActive.HasValue)
            q = q.Where(o => o.IsActive == request.IsActive.Value);
        var list = await q.OrderByDescending(o => o.CreatedAt).ToListAsync(ct);
        return list.Select(OfferMapper.Map);
    }
}

public record GetOfferByIdQuery(int Id) : IRequest<OfferDto?>;

public class GetOfferByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetOfferByIdQuery, OfferDto?>
{
    public async Task<OfferDto?> Handle(GetOfferByIdQuery request, CancellationToken ct)
    {
        var o = await uow.Offers.Query().Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return o is null ? null : OfferMapper.Map(o);
    }
}

/// Check active offers for a product (by productId or promo code)
public record CheckProductOffersQuery(int? ProductId = null, string? PromoCode = null) : IRequest<IEnumerable<OfferDto>>;

public class CheckProductOffersQueryHandler(IUnitOfWork uow) : IRequestHandler<CheckProductOffersQuery, IEnumerable<OfferDto>>
{
    public async Task<IEnumerable<OfferDto>> Handle(CheckProductOffersQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var q = uow.Offers.Query().Include(o => o.Product)
            .Where(o => o.IsActive && (o.StartDate == null || o.StartDate <= now) && (o.EndDate == null || o.EndDate >= now));
        if (request.ProductId.HasValue)
            q = q.Where(o => o.ProductId == null || o.ProductId == request.ProductId.Value);
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
            q = q.Where(o => o.PromoCode == request.PromoCode);
        var list = await q.ToListAsync(ct);
        return list.Select(OfferMapper.Map);
    }
}

// ── Commands ─────────────────────────────────────────────────────────────────

public record CreateOfferCommand(CreateOfferDto Dto) : IRequest<OfferDto>;

public class CreateOfferCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateOfferCommand, OfferDto>
{
    public async Task<OfferDto> Handle(CreateOfferCommand request, CancellationToken ct)
    {
        var o = new Offer
        {
            Name = request.Dto.Name, Description = request.Dto.Description,
            ProductId = request.Dto.ProductId, OfferType = request.Dto.OfferType,
            DiscountValue = request.Dto.DiscountValue, FreeQuantity = request.Dto.FreeQuantity,
            MinimumQuantity = request.Dto.MinimumQuantity, PromoCode = request.Dto.PromoCode,
            IsActive = request.Dto.IsActive, StartDate = request.Dto.StartDate, EndDate = request.Dto.EndDate
        };
        await uow.Offers.AddAsync(o);
        await uow.SaveChangesAsync(ct);
        return OfferMapper.Map(o);
    }
}

public record UpdateOfferCommand(int Id, UpdateOfferDto Dto) : IRequest<bool>;

public class UpdateOfferCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateOfferCommand, bool>
{
    public async Task<bool> Handle(UpdateOfferCommand request, CancellationToken ct)
    {
        var o = await uow.Offers.GetByIdAsync(request.Id);
        if (o is null) return false;
        o.Name = request.Dto.Name; o.Description = request.Dto.Description;
        o.ProductId = request.Dto.ProductId; o.OfferType = request.Dto.OfferType;
        o.DiscountValue = request.Dto.DiscountValue; o.FreeQuantity = request.Dto.FreeQuantity;
        o.MinimumQuantity = request.Dto.MinimumQuantity; o.PromoCode = request.Dto.PromoCode;
        o.IsActive = request.Dto.IsActive; o.StartDate = request.Dto.StartDate; o.EndDate = request.Dto.EndDate;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public record DeleteOfferCommand(int Id) : IRequest<bool>;

public class DeleteOfferCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteOfferCommand, bool>
{
    public async Task<bool> Handle(DeleteOfferCommand request, CancellationToken ct)
    {
        var o = await uow.Offers.GetByIdAsync(request.Id);
        if (o is null) return false;
        uow.Offers.Remove(o);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
