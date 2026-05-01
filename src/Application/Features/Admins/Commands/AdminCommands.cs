using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using DeliverySystem.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Admins.Commands;

// ─── Create Admin ─────────────────────────────────────────────────────────────

/// <summary>أمر إنشاء مسؤول جديد</summary>
public record CreateAdminCommand(CreateAdminDto Dto) : IRequest<AdminDto>;

/// <summary>معالج أمر إنشاء المسؤول</summary>
public class CreateAdminCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateAdminCommand, AdminDto>
{
    public async Task<AdminDto> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = new Admin
        {
            FullName = request.Dto.FullName,
            Username = request.Dto.Username,
            PasswordHash = PasswordHelper.Hash(request.Dto.Password)
        };
        await uow.Admins.AddAsync(admin);
        await uow.SaveChangesAsync(cancellationToken);
        return new AdminDto { Id = admin.Id, FullName = admin.FullName, Username = admin.Username, IsActive = admin.IsActive, CreatedAt = admin.CreatedAt };
    }
}

// ─── Get All Admins ───────────────────────────────────────────────────────────

/// <summary>استعلام جلب جميع المسؤولين</summary>
public record GetAllAdminsQuery : IRequest<IEnumerable<AdminDto>>;

/// <summary>معالج استعلام جلب جميع المسؤولين</summary>
public class GetAllAdminsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllAdminsQuery, IEnumerable<AdminDto>>
{
    public async Task<IEnumerable<AdminDto>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
    {
        var admins = await uow.Admins.Query().Include(a => a.Permissions).ToListAsync(cancellationToken);
        return admins.Select(a => new AdminDto
        {
            Id = a.Id, FullName = a.FullName, Username = a.Username,
            IsActive = a.IsActive, CreatedAt = a.CreatedAt,
            Permissions = a.Permissions.Select(p => new AdminPermissionDto
            {
                Id = p.Id, AdminId = p.AdminId, PageName = p.PageName,
                CanAdd = p.CanAdd, CanEdit = p.CanEdit, CanDelete = p.CanDelete, CanView = p.CanView
            }).ToList()
        });
    }
}

// ─── Delete Admin ─────────────────────────────────────────────────────────────

/// <summary>أمر حذف مسؤول</summary>
public record DeleteAdminCommand(int Id) : IRequest<bool>;

/// <summary>معالج أمر حذف المسؤول</summary>
public class DeleteAdminCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteAdminCommand, bool>
{
    public async Task<bool> Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await uow.Admins.GetByIdAsync(request.Id);
        if (admin is null) return false;
        uow.Admins.Remove(admin);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Toggle Admin Active ──────────────────────────────────────────────────────

/// <summary>أمر تفعيل/تعطيل مسؤول</summary>
public record ToggleAdminActiveCommand(int Id) : IRequest<bool>;

/// <summary>معالج أمر تفعيل/تعطيل المسؤول</summary>
public class ToggleAdminActiveCommandHandler(IUnitOfWork uow) : IRequestHandler<ToggleAdminActiveCommand, bool>
{
    public async Task<bool> Handle(ToggleAdminActiveCommand request, CancellationToken cancellationToken)
    {
        var admin = await uow.Admins.GetByIdAsync(request.Id);
        if (admin is null) return false;
        admin.IsActive = !admin.IsActive;
        uow.Admins.Update(admin);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Save Admin Permissions ───────────────────────────────────────────────────

/// <summary>أمر حفظ صلاحيات مسؤول</summary>
public record SaveAdminPermissionsCommand(int AdminId, List<AdminPermissionDto> Permissions) : IRequest<bool>;

/// <summary>معالج أمر حفظ الصلاحيات — يحذف القديمة ويضيف الجديدة</summary>
public class SaveAdminPermissionsCommandHandler(IUnitOfWork uow) : IRequestHandler<SaveAdminPermissionsCommand, bool>
{
    public async Task<bool> Handle(SaveAdminPermissionsCommand request, CancellationToken cancellationToken)
    {
        // حذف الصلاحيات الحالية
        var existing = await uow.AdminPermissions.FindAsync(p => p.AdminId == request.AdminId);
        foreach (var p in existing)
            uow.AdminPermissions.Remove(p);

        // إضافة الصلاحيات الجديدة
        foreach (var dto in request.Permissions)
        {
            await uow.AdminPermissions.AddAsync(new AdminPermission
            {
                AdminId   = request.AdminId,
                PageName  = dto.PageName,
                CanAdd    = dto.CanAdd,
                CanEdit   = dto.CanEdit,
                CanDelete = dto.CanDelete,
                CanView   = dto.CanView
            });
        }
            await uow.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
