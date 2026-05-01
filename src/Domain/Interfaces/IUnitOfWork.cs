using DeliverySystem.Domain.Entities;

namespace DeliverySystem.Domain.Interfaces;

/// <summary>واجهة وحدة العمل لتنسيق المستودعات وحفظ التغييرات</summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Admin> Admins { get; }
    IRepository<AdminPermission> AdminPermissions { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Employee> Employees { get; }
    IRepository<Category> Categories { get; }
    IRepository<Warehouse> Warehouses { get; }
    IRepository<Product> Products { get; }
    IRepository<Inventory> Inventories { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceDetail> InvoiceDetails { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<ActivityLog> ActivityLogs { get; }
    IRepository<Branch> Branches { get; }
    IRepository<Offer> Offers { get; }
    IRepository<TransferOrder> TransferOrders { get; }
    IRepository<TransferOrderDetail> TransferOrderDetails { get; }
    IRepository<Payment> Payments { get; }
    IRepository<SalesReturn> SalesReturns { get; }
    IRepository<SalesReturnDetail> SalesReturnDetails { get; }
    IRepository<SystemSettings> SystemSettings { get; }
    IRepository<EmployeePermission> EmployeePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
