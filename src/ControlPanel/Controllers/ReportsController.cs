using DeliverySystem.Application.Features.Customers.Queries;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliverySystem.Infrastructure.Data;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller التقارير والتحليلات</summary>
public class ReportsController(IMediator mediator, ApplicationDbContext db) : Controller
{
    // ─── كشف حساب عميل ────────────────────────────────────────────────────────
    public async Task<IActionResult> CustomerStatement(int? customerId, DateTime? from, DateTime? to)
    {
        ViewBag.Customers          = await mediator.Send(new GetAllCustomersQuery());
        ViewBag.SelectedCustomerId = customerId;
        ViewBag.From               = from;
        ViewBag.To                 = to;

        if (customerId.HasValue)
        {
            var customer = await mediator.Send(new GetCustomerByIdQuery(customerId.Value));
            ViewBag.Customer = customer;

            var query = db.Invoices
                .Include(i => i.Employee)
                .Where(i => i.CustomerId == customerId.Value)
                .AsQueryable();

            if (from.HasValue) query = query.Where(i => i.OrderDate >= from.Value);
            if (to.HasValue)   query = query.Where(i => i.OrderDate <= to.Value.AddDays(1));

            var invoices = await query
                .OrderByDescending(i => i.OrderDate)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    i.OrderDate,
                    EmployeeName  = i.Employee != null ? i.Employee.FullName : (string?)null,
                    i.TotalAmount,
                    i.PaidAmount,
                    StatusText    = i.Status == Domain.Enums.InvoiceStatus.Pending     ? "معلق"
                                  : i.Status == Domain.Enums.InvoiceStatus.Accepted    ? "مقبول"
                                  : i.Status == Domain.Enums.InvoiceStatus.Rejected    ? "مرفوض"
                                  : i.Status == Domain.Enums.InvoiceStatus.Completed   ? "مكتمل"
                                  : i.Status == Domain.Enums.InvoiceStatus.AwaitingDelivery ? "في التوصيل"
                                  : "مؤجل"
                })
                .ToListAsync();

            ViewBag.Invoices       = invoices;
            ViewBag.TotalAmount    = invoices.Sum(i => i.TotalAmount);
            ViewBag.TotalPaid      = invoices.Sum(i => i.PaidAmount);
            ViewBag.TotalRemaining = invoices.Sum(i => i.TotalAmount - i.PaidAmount);
        }

        return View();
    }

    // ─── تحليل البيانات ────────────────────────────────────────────────────────
    public async Task<IActionResult> Analytics()
    {
        // مبيعات شهرية آخر 12 شهر
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
        var monthly = await db.Invoices
            .Where(i => i.OrderDate >= twelveMonthsAgo)
            .GroupBy(i => new { i.OrderDate.Year, i.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalAmount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        ViewBag.Months  = monthly.Select(m => $"{m.Month:D2}/{m.Year}").ToList();
        ViewBag.Monthly = monthly.Select(m => m.Total).ToList();

        // توزيع حالات الطلبات
        var statuses = await db.Invoices
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        ViewBag.StatusLabels = statuses.Select(s => s.Status switch
        {
            Domain.Enums.InvoiceStatus.Pending          => "معلق",
            Domain.Enums.InvoiceStatus.Accepted         => "مقبول",
            Domain.Enums.InvoiceStatus.Rejected         => "مرفوض",
            Domain.Enums.InvoiceStatus.Completed        => "مكتمل",
            Domain.Enums.InvoiceStatus.AwaitingDelivery => "في التوصيل",
            Domain.Enums.InvoiceStatus.Deferred         => "مؤجل",
            _ => "غير معروف"
        }).ToList();
        ViewBag.StatusData = statuses.Select(s => s.Count).ToList();

        // أعلى 10 عملاء مبيعاً
        var topCustomers = await db.Invoices
            .Include(i => i.Customer)
            .GroupBy(i => i.Customer!.FullName)
            .Select(g => new { Name = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .OrderByDescending(x => x.Total).Take(10)
            .ToListAsync();

        ViewBag.TopCustomerLabels = topCustomers.Select(c => c.Name).ToList();
        ViewBag.TopCustomerData   = topCustomers.Select(c => c.Total).ToList();

        // أداء الموظفين (المندوبين)
        var reps = await db.Invoices
            .Include(i => i.Customer).ThenInclude(c => c!.Employee)
            .Where(i => i.Customer != null && i.Customer.Employee != null)
            .GroupBy(i => i.Customer!.Employee!.FullName)
            .Select(g => new { Name = g.Key, Total = g.Sum(x => x.PaidAmount) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        ViewBag.RepLabels = reps.Select(r => r.Name).ToList();
        ViewBag.RepData   = reps.Select(r => r.Total).ToList();

        return View();
    }

    // ─── تقرير المبيعات اليومية ────────────────────────────────────────────────
    public async Task<IActionResult> DailySales(DateTime? from, DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddDays(-29);
        var end   = (to ?? DateTime.UtcNow).AddDays(1);

        var daily = await db.Invoices
            .Where(i => i.OrderDate >= start && i.OrderDate < end)
            .GroupBy(i => i.OrderDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalAmount), Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        ViewBag.Labels = daily.Select(d => d.Date.ToString("yyyy/MM/dd")).ToList();
        ViewBag.Totals = daily.Select(d => d.Total).ToList();
        ViewBag.Counts = daily.Select(d => d.Count).ToList();
        ViewBag.GrandTotal = daily.Sum(d => d.Total);
        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To   = (end.AddDays(-1)).ToString("yyyy-MM-dd");
        return View();
    }

    // ─── تقرير تحصيل النقد ────────────────────────────────────────────────────
    public async Task<IActionResult> CashCollection(DateTime? from, DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddDays(-29);
        var end   = (to ?? DateTime.UtcNow).AddDays(1);

        var payments = await db.Payments
            .Include(p => p.PaidByEmployee)
            .Include(p => p.Customer)
            .Where(p => p.PaidAt >= start && p.PaidAt < end)
            .ToListAsync();

        // تجميع حسب الموظف
        var byEmp = payments
            .GroupBy(p => p.PaidByEmployee?.FullName ?? "غير محدد")
            .Select(g => new { Name = g.Key, Total = g.Sum(x => x.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Total).ToList();

        // تجميع حسب النوع
        var byType = payments
            .GroupBy(p => p.Type)
            .Select(g => new
            {
                TypeText = g.Key switch
                {
                    PaymentType.CustomerToDriver           => "عميل → سائق",
                    PaymentType.CustomerToRepresentative   => "عميل → مندوب",
                    PaymentType.DriverToCompany            => "سائق → إدارة",
                    PaymentType.RepresentativeToCompany    => "مندوب → إدارة",
                    _ => "غير محدد"
                },
                Total = g.Sum(x => x.Amount)
            }).ToList();

        ViewBag.EmpLabels   = byEmp.Select(x => x.Name).ToList();
        ViewBag.EmpTotals   = byEmp.Select(x => x.Total).ToList();
        ViewBag.TypeLabels  = byType.Select(x => x.TypeText).ToList();
        ViewBag.TypeTotals  = byType.Select(x => x.Total).ToList();
        ViewBag.Payments    = payments;
        ViewBag.GrandTotal  = payments.Sum(p => p.Amount);
        ViewBag.Verified    = payments.Where(p => p.IsVerified).Sum(p => p.Amount);
        ViewBag.Pending     = payments.Where(p => !p.IsVerified).Sum(p => p.Amount);
        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To   = (end.AddDays(-1)).ToString("yyyy-MM-dd");
        return View();
    }

    // ─── تقرير أداء السائقين ──────────────────────────────────────────────────
    public async Task<IActionResult> DriverPerformance(DateTime? from, DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddDays(-29);
        var end   = (to ?? DateTime.UtcNow).AddDays(1);

        var drivers = await db.Invoices
            .Include(i => i.Employee)
            .Where(i => i.OrderDate >= start && i.OrderDate < end && i.EmployeeId != null)
            .GroupBy(i => new { i.EmployeeId, Name = i.Employee!.FullName })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.Name,
                TotalOrders   = g.Count(),
                Delivered     = g.Count(x => x.Status == Domain.Enums.InvoiceStatus.Delivered || x.Status == Domain.Enums.InvoiceStatus.Completed),
                TotalCollected = g.Sum(x => x.PaidAmount)
            })
            .OrderByDescending(x => x.TotalOrders)
            .ToListAsync();

        ViewBag.DriverLabels     = drivers.Select(d => d.Name).ToList();
        ViewBag.DriverOrders     = drivers.Select(d => d.TotalOrders).ToList();
        ViewBag.DriverDelivered  = drivers.Select(d => d.Delivered).ToList();
        ViewBag.DriverCollected  = drivers.Select(d => d.TotalCollected).ToList();
        ViewBag.Drivers          = drivers;
        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To   = (end.AddDays(-1)).ToString("yyyy-MM-dd");
        return View();
    }

    // ─── تقرير الديون ─────────────────────────────────────────────────────────
    public async Task<IActionResult> DebtReport(string? sortBy, string? sortDir, decimal? minDebt)
    {
        var customers = await db.Customers
            .Where(c => c.IsApproved)
            .Select(c => new
            {
                c.Id, c.FullName, c.Phone, c.StoreName,
                TotalAmount = c.Invoices.Sum(i => i.TotalAmount),
                PaidAmount  = c.Invoices.Sum(i => i.PaidAmount),
                Debt        = c.Invoices.Sum(i => i.TotalAmount - i.PaidAmount),
                LastInvoice = c.Invoices.Max(i => (DateTime?)i.OrderDate)
            })
            .Where(x => x.Debt > 0)
            .ToListAsync();

        if (minDebt.HasValue) customers = customers.Where(c => c.Debt >= minDebt.Value).ToList();

        customers = (sortBy, sortDir) switch
        {
            ("debt",  "asc")  => customers.OrderBy(c => c.Debt).ToList(),
            ("debt",  _)      => customers.OrderByDescending(c => c.Debt).ToList(),
            ("date",  "asc")  => customers.OrderBy(c => c.LastInvoice).ToList(),
            ("date",  _)      => customers.OrderByDescending(c => c.LastInvoice).ToList(),
            _                 => customers.OrderByDescending(c => c.Debt).ToList()
        };

        ViewBag.SortBy   = sortBy ?? "debt";
        ViewBag.SortDir  = sortDir ?? "desc";
        ViewBag.MinDebt  = minDebt;
        ViewBag.Customers = customers;
        ViewBag.TotalDebt = customers.Sum(c => c.Debt);
        return View();
    }

    // ─── تقرير مخصص بالتاريخ ─────────────────────────────────────────────────
    public async Task<IActionResult> CustomReport(DateTime? from, DateTime? to, int? employeeId, int? customerId, string? status)
    {
        ViewBag.Employees  = await mediator.Send(new Application.Features.Employees.Commands.GetAllEmployeesQuery());
        ViewBag.Customers  = await mediator.Send(new Application.Features.Customers.Queries.GetAllCustomersQuery());

        if (!from.HasValue && !to.HasValue && !employeeId.HasValue && !customerId.HasValue && string.IsNullOrEmpty(status))
        {
            ViewBag.From = DateTime.UtcNow.AddDays(-6).ToString("yyyy-MM-dd");
            ViewBag.To   = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return View();
        }

        var query = db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Employee)
            .AsQueryable();

        if (from.HasValue)       query = query.Where(i => i.OrderDate >= from.Value);
        if (to.HasValue)         query = query.Where(i => i.OrderDate <= to.Value.AddDays(1));
        if (employeeId.HasValue) query = query.Where(i => i.EmployeeId == employeeId.Value);
        if (customerId.HasValue) query = query.Where(i => i.CustomerId == customerId.Value);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.Enums.InvoiceStatus>(status, out var st))
            query = query.Where(i => i.Status == st);

        var invoices = await query.OrderByDescending(i => i.OrderDate).ToListAsync();

        ViewBag.Invoices   = invoices;
        ViewBag.TotalAmount = invoices.Sum(i => i.TotalAmount);
        ViewBag.TotalPaid   = invoices.Sum(i => i.PaidAmount);
        ViewBag.TotalDebt   = invoices.Sum(i => i.TotalAmount - i.PaidAmount);
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To   = to?.ToString("yyyy-MM-dd");
        ViewBag.SelectedEmployee = employeeId;
        ViewBag.SelectedCustomer = customerId;
        ViewBag.SelectedStatus   = status;
        return View();
    }

    // ─── تدفق النقد (Customer → Salesperson → Management) ───────────────────
    public async Task<IActionResult> CashFlow(DateTime? from, DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddDays(-29);
        var end   = (to ?? DateTime.UtcNow).AddDays(1);

        var allPayments = await db.Payments
            .Include(p => p.Customer)
            .Include(p => p.PaidByEmployee)
            .Include(p => p.ReceivedByEmployee)
            .Include(p => p.Invoice)
            .Where(p => p.PaidAt >= start && p.PaidAt < end)
            .OrderBy(p => p.PaidAt)
            .ToListAsync();

        ViewBag.CustomerToRep    = allPayments.Where(p => p.Type == PaymentType.CustomerToRepresentative || p.Type == PaymentType.CustomerToDriver).Sum(p => p.Amount);
        ViewBag.RepToCompany     = allPayments.Where(p => p.Type == PaymentType.RepresentativeToCompany || p.Type == PaymentType.DriverToCompany).Sum(p => p.Amount);
        ViewBag.Verified         = allPayments.Where(p => p.IsVerified).Sum(p => p.Amount);
        ViewBag.Payments         = allPayments;
        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To   = (end.AddDays(-1)).ToString("yyyy-MM-dd");
        return View();
    }

    // ─── تحصيلات المندوبين ─────────────────────────────────────────────────────
    public async Task<IActionResult> RepCollections(DateTime? from, DateTime? to, int? employeeId)
    {
        ViewBag.Employees = await mediator.Send(new Application.Features.Employees.Commands.GetAllEmployeesQuery());
        var start = from ?? DateTime.UtcNow.AddDays(-29);
        var end   = (to ?? DateTime.UtcNow).AddDays(1);

        var query = db.Payments
            .Include(p => p.PaidByEmployee)
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Where(p => p.PaidAt >= start && p.PaidAt < end &&
                   (p.Type == PaymentType.CustomerToRepresentative || p.Type == PaymentType.RepresentativeToCompany))
            .AsQueryable();

        if (employeeId.HasValue) query = query.Where(p => p.PaidByEmployeeId == employeeId.Value);

        var payments = await query.OrderByDescending(p => p.PaidAt).ToListAsync();

        var byRep = payments
            .GroupBy(p => p.PaidByEmployee?.FullName ?? "غير محدد")
            .Select(g => new
            {
                Name      = g.Key,
                Collected = g.Where(x => x.Type == PaymentType.CustomerToRepresentative).Sum(x => x.Amount),
                Submitted = g.Where(x => x.Type == PaymentType.RepresentativeToCompany).Sum(x => x.Amount)
            }).OrderByDescending(x => x.Collected).ToList();

        ViewBag.RepLabels    = byRep.Select(r => r.Name).ToList();
        ViewBag.RepCollected = byRep.Select(r => r.Collected).ToList();
        ViewBag.RepSubmitted = byRep.Select(r => r.Submitted).ToList();
        ViewBag.ByRep        = byRep;
        ViewBag.Payments     = payments;
        ViewBag.From = start.ToString("yyyy-MM-dd");
        ViewBag.To   = (end.AddDays(-1)).ToString("yyyy-MM-dd");
        ViewBag.SelectedEmployee = employeeId;
        return View();
    }
}
