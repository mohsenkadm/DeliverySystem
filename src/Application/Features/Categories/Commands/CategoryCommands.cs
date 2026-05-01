using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Categories.Commands;

// ─── Create Category ──────────────────────────────────────────────────────────

/// <summary>أمر إنشاء تصنيف جديد</summary>
public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<CategoryDto>;

/// <summary>معالج أمر إنشاء التصنيف</summary>
public class CreateCategoryCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = new Category { Name = request.Dto.Name };
        await uow.Categories.AddAsync(cat);
        await uow.SaveChangesAsync(cancellationToken);
        return new CategoryDto { Id = cat.Id, Name = cat.Name, CreatedAt = cat.CreatedAt };
    }
}

// ─── Update Category ──────────────────────────────────────────────────────────

/// <summary>أمر تعديل تصنيف</summary>
public record UpdateCategoryCommand(int Id, string Name) : IRequest<bool>;

/// <summary>معالج أمر تعديل التصنيف</summary>
public class UpdateCategoryCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await uow.Categories.GetByIdAsync(request.Id);
        if (cat is null) return false;
        cat.Name = request.Name;
        uow.Categories.Update(cat);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Delete Category ──────────────────────────────────────────────────────────

/// <summary>أمر حذف تصنيف</summary>
public record DeleteCategoryCommand(int Id) : IRequest<bool>;

/// <summary>معالج أمر حذف التصنيف</summary>
public class DeleteCategoryCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var cat = await uow.Categories.GetByIdAsync(request.Id);
        if (cat is null) return false;
        uow.Categories.Remove(cat);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Get All Categories ───────────────────────────────────────────────────────

/// <summary>استعلام جلب جميع التصنيفات</summary>
public record GetAllCategoriesQuery(string? Search = null) : IRequest<IEnumerable<CategoryDto>>;

/// <summary>معالج استعلام جلب جميع التصنيفات</summary>
public class GetAllCategoriesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
{
    public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Categories.Query().Include(c => c.Products).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Name.Contains(request.Search));
        var cats = await query.ToListAsync(cancellationToken);
        return cats.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, ProductCount = c.Products.Count, CreatedAt = c.CreatedAt });
    }
}

/// <summary>استعلام جلب تصنيف بالمعرف</summary>
public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto?>;

/// <summary>معالج استعلام جلب تصنيف بالمعرف</summary>
public class GetCategoryByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await uow.Categories.Query().Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (c is null) return null;
        return new CategoryDto { Id = c.Id, Name = c.Name, ProductCount = c.Products.Count, CreatedAt = c.CreatedAt };
    }
}
