using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Settings.Commands;

public record GetSystemSettingsQuery : IRequest<SystemSettingsDto>;

public class GetSystemSettingsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetSystemSettingsQuery, SystemSettingsDto>
{
    public async Task<SystemSettingsDto> Handle(GetSystemSettingsQuery request, CancellationToken ct)
    {
        var s = await uow.SystemSettings.Query().FirstOrDefaultAsync(ct)
                ?? new SystemSettings();
        return new SystemSettingsDto
        {
            Id = s.Id, SystemName = s.SystemName, LogoPath = s.LogoPath,
            PrimaryColor = s.PrimaryColor, ContactPhone = s.ContactPhone,
            ContactEmail = s.ContactEmail, Address = s.Address,
            FooterText = s.FooterText, UpdatedAt = s.UpdatedAt
        };
    }
}

public record UpdateSystemSettingsCommand(SystemSettingsDto Dto) : IRequest;

public class UpdateSystemSettingsCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateSystemSettingsCommand>
{
    public async Task Handle(UpdateSystemSettingsCommand request, CancellationToken ct)
    {
        var s = await uow.SystemSettings.Query().FirstOrDefaultAsync(ct);
        if (s is null)
        {
            s = new SystemSettings();
            await uow.SystemSettings.AddAsync(s);
        }
        s.SystemName = request.Dto.SystemName;
        s.PrimaryColor = request.Dto.PrimaryColor;
        s.ContactPhone = request.Dto.ContactPhone;
        s.ContactEmail = request.Dto.ContactEmail;
        s.Address = request.Dto.Address;
        s.FooterText = request.Dto.FooterText;
        s.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.Dto.LogoPath)) s.LogoPath = request.Dto.LogoPath;
        await uow.SaveChangesAsync(ct);
    }
}
