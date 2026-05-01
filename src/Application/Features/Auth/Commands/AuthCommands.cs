using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace DeliverySystem.Application.Features.Auth.Commands;

// ─── Password Helper ──────────────────────────────────────────────────────────

public static class PasswordHelper
{
    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32));
        return $"{hash}.{Convert.ToBase64String(salt)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[1]);
        string hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 32));
        return hash == parts[0];
    }
}

// ─── Admin Login ──────────────────────────────────────────────────────────────

public record AdminLoginCommand(AdminLoginDto Dto) : IRequest<AuthResponseDto?>;

public class AdminLoginCommandHandler(IUnitOfWork uow, IJwtService jwt) : IRequestHandler<AdminLoginCommand, AuthResponseDto?>
{
    public async Task<AuthResponseDto?> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var admin = await uow.Admins.FirstOrDefaultAsync(a => a.Username == request.Dto.Username);
        if (admin is null || !admin.IsActive) return null;
        if (!PasswordHelper.Verify(request.Dto.Password, admin.PasswordHash)) return null;
        return new AuthResponseDto
        {
            Token = jwt.GenerateToken(admin.Id, admin.Username, "Admin"),
            Username = admin.Username, FullName = admin.FullName, Role = "Admin", UserId = admin.Id
        };
    }
}

// ─── Customer Login ───────────────────────────────────────────────────────────

public record CustomerLoginCommand(CustomerLoginDto Dto) : IRequest<AuthResponseDto?>;

public class CustomerLoginCommandHandler(IUnitOfWork uow, IJwtService jwt) : IRequestHandler<CustomerLoginCommand, AuthResponseDto?>
{
    public async Task<AuthResponseDto?> Handle(CustomerLoginCommand request, CancellationToken cancellationToken)
    {
        var customer = await uow.Customers.FirstOrDefaultAsync(c => c.Username == request.Dto.Username);
        if (customer is null || !PasswordHelper.Verify(request.Dto.Password, customer.PasswordHash)) return null;
        if (!customer.IsApproved) return null;
        return new AuthResponseDto
        {
            Token = jwt.GenerateToken(customer.Id, customer.Username, "Customer"),
            Username = customer.Username, FullName = customer.FullName, Role = "Customer", UserId = customer.Id
        };
    }
}

// ─── Employee Login ───────────────────────────────────────────────────────────

public record EmployeeLoginCommand(EmployeeLoginDto Dto) : IRequest<AuthResponseDto?>;

public class EmployeeLoginCommandHandler(IUnitOfWork uow, IJwtService jwt) : IRequestHandler<EmployeeLoginCommand, AuthResponseDto?>
{
    public async Task<AuthResponseDto?> Handle(EmployeeLoginCommand request, CancellationToken cancellationToken)
    {
        var emp = await uow.Employees.FirstOrDefaultAsync(e => e.Username == request.Dto.Username);
        if (emp is null || !emp.IsActive || !PasswordHelper.Verify(request.Dto.Password, emp.PasswordHash)) return null;
        return new AuthResponseDto
        {
            Token = jwt.GenerateToken(emp.Id, emp.Username, emp.EmployeeType),
            Username = emp.Username, FullName = emp.FullName, Role = emp.EmployeeType, UserId = emp.Id
        };
    }
}
