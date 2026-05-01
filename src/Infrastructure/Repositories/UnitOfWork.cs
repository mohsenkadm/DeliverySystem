using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using DeliverySystem.Infrastructure.Data;

namespace DeliverySystem.Infrastructure.Repositories;

/// <summary>وحدة العمل التي تُنسّق جميع المستودعات وتحفظ التغييرات دفعةً واحدة</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Admin> Admins { get; }
    public IRepository<AdminPermission> AdminPermissions { get; }
    public IRepository<Customer> Customers { get; }
    public IRepository<Employee> Employees { get; }
    public IRepository<Category> Categories { get; }
    public IRepository<Warehouse> Warehouses { get; }
    public IRepository<Product> Products { get; }
    public IRepository<Inventory> Inventories { get; }
    public IRepository<Invoice> Invoices { get; }
    public IRepository<InvoiceDetail> InvoiceDetails { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<ActivityLog> ActivityLogs { get; }
    public IRepository<Branch> Branches { get; }
    public IRepository<Offer> Offers { get; }
    public IRepository<TransferOrder> TransferOrders { get; }
    public IRepository<TransferOrderDetail> TransferOrderDetails { get; }
    public IRepository<Payment> Payments { get; }
    public IRepository<SalesReturn> SalesReturns { get; }
    public IRepository<SalesReturnDetail> SalesReturnDetails { get; }
    public IRepository<SystemSettings> SystemSettings { get; }
    public IRepository<EmployeePermission> EmployeePermissions { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Admins = new GenericRepository<Admin>(context);
        AdminPermissions = new GenericRepository<AdminPermission>(context);
        Customers = new GenericRepository<Customer>(context);
        Employees = new GenericRepository<Employee>(context);
        Categories = new GenericRepository<Category>(context);
        Warehouses = new GenericRepository<Warehouse>(context);
        Products = new GenericRepository<Product>(context);
        Inventories = new GenericRepository<Inventory>(context);
        Invoices = new GenericRepository<Invoice>(context);
        InvoiceDetails = new GenericRepository<InvoiceDetail>(context);
        Notifications = new GenericRepository<Notification>(context);
        ActivityLogs = new GenericRepository<ActivityLog>(context);
        Branches = new GenericRepository<Branch>(context);
        Offers = new GenericRepository<Offer>(context);
        TransferOrders = new GenericRepository<TransferOrder>(context);
        TransferOrderDetails = new GenericRepository<TransferOrderDetail>(context);
        Payments = new GenericRepository<Payment>(context);
        SalesReturns = new GenericRepository<SalesReturn>(context);
        SalesReturnDetails = new GenericRepository<SalesReturnDetail>(context);
        SystemSettings = new GenericRepository<SystemSettings>(context);
        EmployeePermissions = new GenericRepository<EmployeePermission>(context);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
