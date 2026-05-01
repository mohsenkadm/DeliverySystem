using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Infrastructure.Data;

/// <summary>سياق قاعدة البيانات الرئيسي للتطبيق</summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<AdminPermission> AdminPermissions => Set<AdminPermission>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceDetail> InvoiceDetails => Set<InvoiceDetail>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<TransferOrder> TransferOrders => Set<TransferOrder>();
    public DbSet<TransferOrderDetail> TransferOrderDetails => Set<TransferOrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnDetail> SalesReturnDetails => Set<SalesReturnDetail>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<EmployeePermission> EmployeePermissions => Set<EmployeePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Admin
        modelBuilder.Entity<Admin>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });

        // AdminPermission
        modelBuilder.Entity<AdminPermission>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Admin).WithMany(a => a.Permissions)
             .HasForeignKey(x => x.AdminId).OnDelete(DeleteBehavior.Cascade);
        });

        // Employee (with self-referencing supervisor)
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.BranchEntity).WithMany(b => b.Employees)
             .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Supervisor).WithMany()
             .HasForeignKey(x => x.SupervisorId).OnDelete(DeleteBehavior.NoAction);
        });

        // Customer
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.Employee).WithMany(emp => emp.Customers)
             .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.BranchEntity).WithMany(b => b.Customers)
             .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
        });

        // Category
        modelBuilder.Entity<Category>(e => e.HasKey(x => x.Id));

        // Warehouse (with optional rep owner)
        modelBuilder.Entity<Warehouse>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.BranchEntity).WithMany(b => b.Warehouses)
             .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.OwnerEmployee).WithMany()
             .HasForeignKey(x => x.OwnerEmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.WholesalePrice).HasPrecision(18, 2);
            e.Property(x => x.RetailPrice).HasPrecision(18, 2);
            e.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
            e.HasOne(x => x.Category).WithMany(c => c.Products)
             .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // Inventory
        modelBuilder.Entity<Inventory>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Product).WithMany(p => p.Inventories)
             .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Warehouse).WithMany(w => w.Inventories)
             .HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        });

        // Invoice (with approved-by employee — no cascade to avoid multiple paths)
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.Property(x => x.PaidAmount).HasPrecision(18, 2);
            e.Ignore(x => x.RemainingAmount);
            e.HasOne(x => x.Customer).WithMany(c => c.Invoices)
             .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Employee).WithMany(emp => emp.Invoices)
             .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.ApprovedByEmployee).WithMany()
             .HasForeignKey(x => x.ApprovedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.BranchEntity).WithMany(b => b.Invoices)
             .HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
        });

        // InvoiceDetail
        modelBuilder.Entity<InvoiceDetail>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Ignore(x => x.SubTotal);
            e.HasOne(x => x.Invoice).WithMany(i => i.Details)
             .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(p => p.InvoiceDetails)
             .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
        });

        // ActivityLog
        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).HasMaxLength(200).IsRequired();
        });

        // Branch
        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
        });

        // Offer
        modelBuilder.Entity<Offer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.DiscountValue).HasPrecision(18, 2);
            e.HasOne(x => x.Product).WithMany()
             .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
        });

        // TransferOrder
        modelBuilder.Entity<TransferOrder>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.FromWarehouse).WithMany()
             .HasForeignKey(x => x.FromWarehouseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ToWarehouse).WithMany()
             .HasForeignKey(x => x.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RequestedByEmployee).WithMany()
             .HasForeignKey(x => x.RequestedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ApprovedByEmployee).WithMany()
             .HasForeignKey(x => x.ApprovedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        // TransferOrderDetail
        modelBuilder.Entity<TransferOrderDetail>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.TransferOrder).WithMany(t => t.Details)
             .HasForeignKey(x => x.TransferOrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany()
             .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasOne(x => x.Invoice).WithMany(i => i.Payments)
             .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Customer).WithMany()
             .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.PaidByEmployee).WithMany()
             .HasForeignKey(x => x.PaidByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ReceivedByEmployee).WithMany()
             .HasForeignKey(x => x.ReceivedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        // SalesReturn
        modelBuilder.Entity<SalesReturn>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Invoice).WithMany()
             .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.RequestedByEmployee).WithMany()
             .HasForeignKey(x => x.RequestedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ApprovedByManager).WithMany()
             .HasForeignKey(x => x.ApprovedByManagerId).OnDelete(DeleteBehavior.NoAction);
        });

        // SalesReturnDetail
        modelBuilder.Entity<SalesReturnDetail>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.SalesReturn).WithMany(r => r.Details)
             .HasForeignKey(x => x.SalesReturnId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany()
             .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // SystemSettings
        modelBuilder.Entity<SystemSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SystemName).HasMaxLength(200).IsRequired();
        });

        // EmployeePermission
        modelBuilder.Entity<EmployeePermission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PageName).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Employee).WithMany()
             .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });

        // Seed: SystemSettings default
        modelBuilder.Entity<SystemSettings>().HasData(new SystemSettings
        {
            Id = 1,
            SystemName = "نظام ادارة المبيعات والتوصيل",
            UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed: Admin default
        modelBuilder.Entity<Admin>().HasData(new Admin
        {
            Id = 1,
            FullName = "المدير العام",
            Username = "admin",
            PasswordHash = "3rFqSvbCrLX4VGzPtHxdOA==.KFBMXsqHJcWr1A7UdTbWpQ==",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
