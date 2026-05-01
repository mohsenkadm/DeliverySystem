using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace DeliverySystem.Application.Features.Customers.Commands;

// ─── Create Customer ──────────────────────────────────────────────────────────

public record CreateCustomerCommand(CreateCustomerDto Dto, bool IsApproved = false) : IRequest<CustomerDto>;

public class CreateCustomerCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            FullName       = request.Dto.FullName,
            StoreName      = request.Dto.StoreName,
            Description    = request.Dto.Description,
            Phone          = request.Dto.Phone,
            Address        = request.Dto.Address,
            ClientType     = request.Dto.ClientType,
            Latitude       = request.Dto.Latitude,
            Longitude      = request.Dto.Longitude,
            Region         = request.Dto.Region,
            Branch         = request.Dto.Branch,
            StoreImagePath = request.Dto.StoreImagePath,
            Username       = request.Dto.Username,
            PasswordHash   = HashPassword(request.Dto.Password),
            EmployeeId     = request.Dto.EmployeeId,
            IsApproved     = request.IsApproved
        };
        await uow.Customers.AddAsync(customer);
        await uow.SaveChangesAsync(cancellationToken);
        return MapToDto(customer);
    }

    private static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8))
               + "." + Convert.ToBase64String(salt);
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        Id = c.Id, FullName = c.FullName, StoreName = c.StoreName, Description = c.Description,
        Phone = c.Phone, Address = c.Address, ClientType = c.ClientType,
        Latitude = c.Latitude, Longitude = c.Longitude, Region = c.Region, Branch = c.Branch,
        StoreImagePath = c.StoreImagePath, Username = c.Username,
        IsApproved = c.IsApproved, EmployeeId = c.EmployeeId, CreatedAt = c.CreatedAt
    };
}

// ─── Update Customer ──────────────────────────────────────────────────────────

public record UpdateCustomerCommand(int Id, UpdateCustomerDto Dto) : IRequest<bool>;

public class UpdateCustomerCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateCustomerCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await uow.Customers.GetByIdAsync(request.Id);
        if (customer is null) return false;
        customer.FullName       = request.Dto.FullName;
        customer.StoreName      = request.Dto.StoreName;
        customer.Description    = request.Dto.Description;
        customer.Phone          = request.Dto.Phone;
        customer.Address        = request.Dto.Address;
        customer.ClientType     = request.Dto.ClientType;
        customer.Latitude       = request.Dto.Latitude;
        customer.Longitude      = request.Dto.Longitude;
        customer.Region         = request.Dto.Region;
        customer.Branch         = request.Dto.Branch;
        customer.StoreImagePath = request.Dto.StoreImagePath;
        customer.EmployeeId     = request.Dto.EmployeeId;
        uow.Customers.Update(customer);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Delete Customer ──────────────────────────────────────────────────────────

public record DeleteCustomerCommand(int Id) : IRequest<bool>;

public class DeleteCustomerCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteCustomerCommand, bool>
{
    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await uow.Customers.GetByIdAsync(request.Id);
        if (customer is null) return false;
        uow.Customers.Remove(customer);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Approve Customer ─────────────────────────────────────────────────────────

public record ApproveCustomerCommand(int Id) : IRequest<bool>;

public class ApproveCustomerCommandHandler(IUnitOfWork uow) : IRequestHandler<ApproveCustomerCommand, bool>
{
    public async Task<bool> Handle(ApproveCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await uow.Customers.GetByIdAsync(request.Id);
        if (customer is null) return false;
        customer.IsApproved = true;
        uow.Customers.Update(customer);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
