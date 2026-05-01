using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Products.Commands;

// ─── Create Product ───────────────────────────────────────────────────────────

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;

public class CreateProductCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name               = request.Dto.Name,
            Code               = $"PRD-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Description        = request.Dto.Description,
            WholesalePrice     = request.Dto.WholesalePrice,
            RetailPrice        = request.Dto.RetailPrice,
            DiscountPercentage = request.Dto.DiscountPercentage,
            CartonType         = request.Dto.CartonType,
            BaseQuantity       = request.Dto.BaseQuantity,
            ProductionDate     = request.Dto.ProductionDate,
            ExpirationDate     = request.Dto.ExpirationDate,
            ImagePath          = request.Dto.ImagePath,
            CategoryId         = request.Dto.CategoryId
        };
        await uow.Products.AddAsync(product);
        await uow.SaveChangesAsync(cancellationToken);
        return ProductMapper.Map(product);
    }
}

// ─── Update Product ───────────────────────────────────────────────────────────

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest<bool>;

public class UpdateProductCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await uow.Products.GetByIdAsync(request.Id);
        if (product is null) return false;
        product.Name               = request.Dto.Name;
        product.Description        = request.Dto.Description;
        product.WholesalePrice     = request.Dto.WholesalePrice;
        product.RetailPrice        = request.Dto.RetailPrice;
        product.DiscountPercentage = request.Dto.DiscountPercentage;
        product.CartonType         = request.Dto.CartonType;
        product.BaseQuantity       = request.Dto.BaseQuantity;
        product.ProductionDate     = request.Dto.ProductionDate;
        product.ExpirationDate     = request.Dto.ExpirationDate;
        product.ImagePath          = request.Dto.ImagePath;
        product.CategoryId         = request.Dto.CategoryId;
        uow.Products.Update(product);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Delete Product ───────────────────────────────────────────────────────────

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await uow.Products.GetByIdAsync(request.Id);
        if (product is null) return false;
        uow.Products.Remove(product);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Get All Products ─────────────────────────────────────────────────────────

public record GetAllProductsQuery(string? Search = null, int? CategoryId = null) : IRequest<IEnumerable<ProductDto>>;

public class GetAllProductsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Products.Query()
            .Include(p => p.Category)
            .Include(p => p.Inventories).ThenInclude(i => i.Warehouse)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || (p.Code != null && p.Code.Contains(request.Search)));
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId);
        var products = await query.ToListAsync(cancellationToken);
        return products.Select(ProductMapper.Map);
    }
}

// ─── Get Product By Id ────────────────────────────────────────────────────────

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await uow.Products.Query()
            .Include(x => x.Category)
            .Include(x => x.Inventories).ThenInclude(i => i.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        return p is null ? null : ProductMapper.Map(p);
    }
}

// ─── Mapper ───────────────────────────────────────────────────────────────────

public static class ProductMapper
{
    public static ProductDto Map(Product p) => new()
    {
        Id = p.Id, Name = p.Name, Code = p.Code, Description = p.Description,
        WholesalePrice = p.WholesalePrice, RetailPrice = p.RetailPrice,
        DiscountPercentage = p.DiscountPercentage, CartonType = p.CartonType,
        BaseQuantity = p.BaseQuantity, ProductionDate = p.ProductionDate,
        ExpirationDate = p.ExpirationDate, ImagePath = p.ImagePath,
        CategoryId = p.CategoryId, CategoryName = p.Category?.Name, CreatedAt = p.CreatedAt,
        Inventories = p.Inventories.Select(i => new ProductInventoryDto
        {
            WarehouseId = i.WarehouseId, WarehouseName = i.Warehouse?.Name ?? string.Empty, Quantity = i.Quantity
        }).ToList()
    };
}
