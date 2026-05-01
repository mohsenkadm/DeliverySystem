using DeliverySystem.Application.Features.ActivityLogs.Commands;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliverySystem.Infrastructure.Data;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller لوحة القيادة الرئيسية</summary>
public class DashboardController(IMediator mediator, ApplicationDbContext db) : Controller
{
    /// <summary>الصفحة الرئيسية لـ Dashboard مع الإحصائيات</summary>
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        ViewBag.TotalSalesToday = await db.Invoices
            .Where(i => i.OrderDate.Date == today)
            .SumAsync(i => i.TotalAmount);
        ViewBag.TotalOrders = await db.Invoices.CountAsync();
        ViewBag.ActiveDeliveries = await db.Invoices
            .CountAsync(i => i.Status == Domain.Enums.InvoiceStatus.AwaitingDelivery);
        ViewBag.TotalDebt = await db.Invoices.SumAsync(i => i.TotalAmount - i.PaidAmount);
        ViewBag.TotalCustomers = await db.Customers.CountAsync();
        ViewBag.TotalEmployees = await db.Employees.CountAsync();
        ViewBag.TotalProducts = await db.Products.CountAsync();

        // آخر 10 فواتير
        var recentInvoices = await db.Invoices
            .Include(i => i.Customer).Include(i => i.Employee)
            .OrderByDescending(i => i.OrderDate)
            .Take(10).ToListAsync();
        ViewBag.RecentInvoices = recentInvoices;

        // بيانات المخطط - مبيعات آخر 7 أيام
        var salesData = await db.Invoices
            .Where(i => i.OrderDate >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(i => i.OrderDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .OrderBy(x => x.Date).ToListAsync();
        ViewBag.SalesLabels = salesData.Select(x => x.Date.ToString("dd/MM")).ToList();
        ViewBag.SalesData = salesData.Select(x => x.Total).ToList();

        // بيانات Pie Chart - توزيع التصنيفات
        var categoryData = await db.Products
            .GroupBy(p => p.Category.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();
        ViewBag.CategoryLabels = categoryData.Select(x => x.Name).ToList();
        ViewBag.CategoryData = categoryData.Select(x => x.Count).ToList();

        // آخر النشاطات
        var activities = await db.ActivityLogs
            .OrderByDescending(l => l.Timestamp).Take(10).ToListAsync();
        ViewBag.Activities = activities;

        return View();
    }
}
