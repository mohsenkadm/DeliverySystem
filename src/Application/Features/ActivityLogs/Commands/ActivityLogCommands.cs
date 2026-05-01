using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.ActivityLogs.Commands;

/// <summary>أمر تسجيل نشاط جديد</summary>
public record LogActivityCommand(string Action, string PerformedBy, string UserRole, string? Details = null)
    : IRequest<Unit>;

/// <summary>معالج أمر تسجيل النشاط</summary>
public class LogActivityCommandHandler(IUnitOfWork uow) : IRequestHandler<LogActivityCommand, Unit>
{
    public async Task<Unit> Handle(LogActivityCommand request, CancellationToken cancellationToken)
    {
        await uow.ActivityLogs.AddAsync(new ActivityLog
        {
            Action = request.Action, PerformedBy = request.PerformedBy,
            UserRole = request.UserRole, Details = request.Details,
            Timestamp = DateTime.UtcNow
        });
        await uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

/// <summary>استعلام جلب سجلات النشاط</summary>
public record GetActivityLogsQuery(string? Action = null, string? PerformedBy = null, DateTime? From = null, DateTime? To = null)
    : IRequest<IEnumerable<ActivityLogDto>>;

/// <summary>معالج استعلام جلب سجلات النشاط</summary>
public class GetActivityLogsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetActivityLogsQuery, IEnumerable<ActivityLogDto>>
{
    public async Task<IEnumerable<ActivityLogDto>> Handle(GetActivityLogsQuery request, CancellationToken cancellationToken)
    {
        var query = uow.ActivityLogs.Query().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Action)) query = query.Where(l => l.Action.Contains(request.Action));
        if (!string.IsNullOrWhiteSpace(request.PerformedBy)) query = query.Where(l => l.PerformedBy.Contains(request.PerformedBy));
        if (request.From.HasValue) query = query.Where(l => l.Timestamp >= request.From);
        if (request.To.HasValue) query = query.Where(l => l.Timestamp <= request.To);
        var logs = await query.OrderByDescending(l => l.Timestamp).ToListAsync(cancellationToken);
        return logs.Select(l => new ActivityLogDto
        {
            Id = l.Id, Action = l.Action, PerformedBy = l.PerformedBy,
            UserRole = l.UserRole, Timestamp = l.Timestamp, Details = l.Details
        });
    }
}
