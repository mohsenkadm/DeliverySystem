using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliverySystem.Infrastructure.Data;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller تسوية الديون</summary>
public class DebtsController(IMediator mediator, ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int? customerId)
    {
        var query = db.Invoices
            .Include(i => i.Customer).ThenInclude(c => c!.Employee)
            .Where(i => i.TotalAmount - i.PaidAmount > 0)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId.Value);

        var debts = await query
            .OrderByDescending(i => i.OrderDate)
            .Select(i => new
            {
                InvoiceId         = i.Id,
                InvoiceNumber     = i.InvoiceNumber,
                OrderDate         = i.OrderDate,
                CustomerName      = i.Customer != null ? i.Customer.FullName : string.Empty,
                EmployeeName      = i.Customer != null && i.Customer.Employee != null
                                    ? i.Customer.Employee.FullName : (string?)null,
                TotalAmount       = i.TotalAmount,
                PaidAmount        = i.PaidAmount,
                RemainingAmount   = i.TotalAmount - i.PaidAmount
            })
            .ToListAsync();

        ViewBag.Debts              = debts;
        ViewBag.TotalDebt          = debts.Sum(d => d.RemainingAmount);
        ViewBag.CustomerWithDebt   = debts.Select(d => d.CustomerName).Distinct().Count();
        ViewBag.InvoicesWithDebt   = debts.Count;
        ViewBag.Customers          = await mediator.Send(new GetAllCustomersQuery());

        return View();
    }
}
