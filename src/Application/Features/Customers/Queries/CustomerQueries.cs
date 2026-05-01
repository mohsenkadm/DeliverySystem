using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Customers.Queries;

// ─── Get All Customers ────────────────────────────────────────────────────────

public record GetAllCustomersQuery(string? Search = null, int? EmployeeId = null, bool? IsApproved = null)
    : IRequest<IEnumerable<CustomerDto>>;

public class GetAllCustomersQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
{
    public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Customers.Query()
            .Include(c => c.Employee)
            .Include(c => c.Invoices)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.FullName.Contains(request.Search) || c.Phone.Contains(request.Search)
                                  || (c.StoreName != null && c.StoreName.Contains(request.Search)));

        if (request.EmployeeId.HasValue)
            query = query.Where(c => c.EmployeeId == request.EmployeeId);

        if (request.IsApproved.HasValue)
            query = query.Where(c => c.IsApproved == request.IsApproved);

        var customers = await query.ToListAsync(cancellationToken);
        return customers.Select(c => new CustomerDto
        {
            Id = c.Id, FullName = c.FullName, StoreName = c.StoreName, Description = c.Description,
            Phone = c.Phone, Address = c.Address, ClientType = c.ClientType,
            Latitude = c.Latitude, Longitude = c.Longitude, Region = c.Region, Branch = c.Branch,
            StoreImagePath = c.StoreImagePath, Username = c.Username,
            IsApproved = c.IsApproved, EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee?.FullName,
            CreatedAt = c.CreatedAt,
            InvoiceCount = c.Invoices.Count,
            TotalInvoices = c.Invoices.Sum(i => i.TotalAmount),
            TotalPaid = c.Invoices.Sum(i => i.PaidAmount),
            TotalDebt = c.Invoices.Sum(i => i.TotalAmount - i.PaidAmount)
        });
    }
}

// ─── Get Customer By Id ───────────────────────────────────────────────────────

public record GetCustomerByIdQuery(int Id) : IRequest<CustomerDto?>;

public class GetCustomerByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await uow.Customers.Query()
            .Include(x => x.Employee)
            .Include(x => x.Invoices)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (c is null) return null;
        return new CustomerDto
        {
            Id = c.Id, FullName = c.FullName, StoreName = c.StoreName, Description = c.Description,
            Phone = c.Phone, Address = c.Address, ClientType = c.ClientType,
            Latitude = c.Latitude, Longitude = c.Longitude, Region = c.Region, Branch = c.Branch,
            StoreImagePath = c.StoreImagePath, Username = c.Username,
            IsApproved = c.IsApproved, EmployeeId = c.EmployeeId,
            EmployeeName = c.Employee?.FullName,
            CreatedAt = c.CreatedAt,
            InvoiceCount = c.Invoices.Count,
            TotalInvoices = c.Invoices.Sum(i => i.TotalAmount),
            TotalPaid = c.Invoices.Sum(i => i.PaidAmount),
            TotalDebt = c.Invoices.Sum(i => i.TotalAmount - i.PaidAmount)
        };
    }
}
