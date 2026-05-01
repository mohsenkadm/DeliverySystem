using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Customers.Commands;
using DeliverySystem.Application.Features.Customers.Queries;
using DeliverySystem.Application.Features.Employees.Commands;
using DeliverySystem.Application.Features.ActivityLogs.Commands;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller إدارة العملاء في لوحة التحكم</summary>
public class CustomersController(IMediator mediator) : Controller
{
    public static readonly List<string> AllRegions = EmployeesController.AllRegions;

    public async Task<IActionResult> Index(string? search, int? empId, bool? isApproved)
    {
        var employees = await mediator.Send(new GetAllEmployeesQuery());
        ViewBag.Employees = employees;
        var customers = await mediator.Send(new GetAllCustomersQuery(search, empId, isApproved));
        ViewBag.Search = search; ViewBag.EmpId = empId; ViewBag.IsApproved = isApproved;
        return View(customers);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Employees = await mediator.Send(new GetAllEmployeesQuery());
        ViewBag.AllRegions = AllRegions;
        return View(new CreateCustomerDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto, IFormFile? storeImage)
    {
        dto.StoreImagePath = await SaveImageAsync(storeImage);
        if (!ModelState.IsValid)
        {
            ViewBag.Employees = await mediator.Send(new GetAllEmployeesQuery());
            ViewBag.AllRegions = AllRegions;
            return View(dto);
        }
        await mediator.Send(new CreateCustomerCommand(dto, IsApproved: false));
        await mediator.Send(new LogActivityCommand("إضافة عميل", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم إضافة عميل جديد: {dto.FullName}"));
        TempData["Success"] = "تم إضافة العميل بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await mediator.Send(new GetCustomerByIdQuery(id));
        if (customer is null) return NotFound();
        ViewBag.Employees = await mediator.Send(new GetAllEmployeesQuery());
        ViewBag.AllRegions = AllRegions;
        ViewBag.CustomerId = id;
        ViewBag.CurrentImage = customer.StoreImagePath;
        return View(new UpdateCustomerDto
        {
            FullName = customer.FullName, StoreName = customer.StoreName, Description = customer.Description,
            Phone = customer.Phone, Address = customer.Address, ClientType = customer.ClientType,
            Latitude = customer.Latitude, Longitude = customer.Longitude,
            Region = customer.Region, Branch = customer.Branch,
            EmployeeId = customer.EmployeeId, StoreImagePath = customer.StoreImagePath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCustomerDto dto, IFormFile? storeImage)
    {
        var newImg = await SaveImageAsync(storeImage);
        if (newImg != null) dto.StoreImagePath = newImg;
        if (!ModelState.IsValid)
        {
            ViewBag.Employees = await mediator.Send(new GetAllEmployeesQuery()); ViewBag.AllRegions = AllRegions; return View(dto);
        }
        await mediator.Send(new UpdateCustomerCommand(id, dto));
        await mediator.Send(new LogActivityCommand("تعديل عميل", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم تعديل بيانات العميل رقم {id}"));
        TempData["Success"] = "تم تعديل بيانات العميل";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await mediator.Send(new GetCustomerByIdQuery(id));
        if (customer is null) return NotFound();
        return View(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        await mediator.Send(new ApproveCustomerCommand(id));
        await mediator.Send(new LogActivityCommand("موافقة عميل", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تمت الموافقة على العميل رقم {id}"));
        TempData["Success"] = "تمت الموافقة على العميل";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteCustomerCommand(id));
        await mediator.Send(new LogActivityCommand("حذف عميل", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم حذف العميل رقم {id}"));
        TempData["Success"] = "تم حذف العميل";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var customers = await mediator.Send(new GetAllCustomersQuery());
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("العملاء");
        ws.Cell(1,1).Value="الاسم"; ws.Cell(1,2).Value="اسم المتجر"; ws.Cell(1,3).Value="الهاتف";
        ws.Cell(1,4).Value="العنوان"; ws.Cell(1,5).Value="النوع"; ws.Cell(1,6).Value="المنطقة";
        ws.Cell(1,7).Value="المندوب"; ws.Cell(1,8).Value="الحالة"; ws.Cell(1,9).Value="إجمالي الفواتير";
        int row = 2;
        foreach (var c in customers)
        {
            ws.Cell(row,1).Value = c.FullName; ws.Cell(row,2).Value = c.StoreName ?? "-";
            ws.Cell(row,3).Value = c.Phone; ws.Cell(row,4).Value = c.Address;
            ws.Cell(row,5).Value = c.ClientType == "Wholesale" ? "جملة" : "فرد";
            ws.Cell(row,6).Value = c.Region ?? "-"; ws.Cell(row,7).Value = c.EmployeeName ?? "-";
            ws.Cell(row,8).Value = c.IsApproved ? "موافق عليه" : "في انتظار الموافقة";
            ws.Cell(row,9).Value = c.TotalInvoices; row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "العملاء.xlsx");
    }

    private static async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var dir = Path.Combine("wwwroot", "uploads", "customers");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var fs = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
        await file.CopyToAsync(fs);
        return $"/uploads/customers/{fileName}";
    }
}
