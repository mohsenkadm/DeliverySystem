using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Notifications.Commands;

/// <summary>أمر إنشاء إشعار جديد وحفظه</summary>
public record CreateNotificationCommand(string Title, string Body, NotificationTarget Target, int? TargetUserId = null)
    : IRequest<NotificationDto>;

/// <summary>معالج أمر إنشاء الإشعار</summary>
public class CreateNotificationCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Title = request.Title, Body = request.Body,
            Target = request.Target, TargetUserId = request.TargetUserId,
            IsRead = false, CreatedAt = DateTime.UtcNow
        };
        await uow.Notifications.AddAsync(notification);
        await uow.SaveChangesAsync(cancellationToken);
        return new NotificationDto
        {
            Id = notification.Id, Title = notification.Title, Body = notification.Body,
            Target = notification.Target, TargetUserId = notification.TargetUserId,
            IsRead = notification.IsRead, CreatedAt = notification.CreatedAt
        };
    }
}

/// <summary>أمر تحديد الإشعار كمقروء</summary>
public record MarkNotificationReadCommand(int Id) : IRequest<bool>;

/// <summary>معالج أمر تحديد الإشعار كمقروء</summary>
public class MarkNotificationReadCommandHandler(IUnitOfWork uow) : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var n = await uow.Notifications.GetByIdAsync(request.Id);
        if (n is null) return false;
        n.IsRead = true;
        uow.Notifications.Update(n);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>استعلام جلب جميع الإشعارات للوحة التحكم</summary>
public record GetAllNotificationsAdminQuery(int Page = 1, int PageSize = 50) : IRequest<List<NotificationDto>>;

public class GetAllNotificationsAdminQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetAllNotificationsAdminQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetAllNotificationsAdminQuery request, CancellationToken ct)
        => await uow.Notifications.Query()
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id, Title = n.Title, Body = n.Body, Target = n.Target,
                TargetUserId = n.TargetUserId, IsRead = n.IsRead, CreatedAt = n.CreatedAt
            }).ToListAsync(ct);
}

/// <summary>أمر إرسال إشعار جماعي أو مخصص</summary>
public record SendBulkNotificationCommand(string Title, string Body, NotificationTarget Target, int? TargetUserId = null) : IRequest;

public class SendBulkNotificationCommandHandler(IUnitOfWork uow) : IRequestHandler<SendBulkNotificationCommand>
{
    public async Task Handle(SendBulkNotificationCommand request, CancellationToken ct)
    {
        await uow.Notifications.AddAsync(new Notification
        {
            Title = request.Title, Body = request.Body,
            Target = request.Target, TargetUserId = request.TargetUserId,
            CreatedAt = DateTime.UtcNow
        });
        await uow.SaveChangesAsync(ct);
    }
}

/// <summary>استعلام جلب إشعارات مستخدم</summary>
public record GetNotificationsQuery(NotificationTarget Target, int? UserId = null) : IRequest<IEnumerable<NotificationDto>>;

/// <summary>معالج استعلام جلب الإشعارات</summary>
public class GetNotificationsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationDto>>
{
    public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Notifications.Query()
            .Where(n => n.Target == request.Target)
            .AsQueryable();
        if (request.UserId.HasValue)
            query = query.Where(n => n.TargetUserId == null || n.TargetUserId == request.UserId);
        var list = await query.OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
        return list.Select(n => new NotificationDto
        {
            Id = n.Id, Title = n.Title, Body = n.Body, Target = n.Target,
            TargetUserId = n.TargetUserId, IsRead = n.IsRead, CreatedAt = n.CreatedAt
        });
    }
}
