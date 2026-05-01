using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Employees.Commands;
using DeliverySystem.Application.Features.ActivityLogs.Commands;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// ── System pages available for permission assignment ──────────────────────────

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller إدارة الموظفين (يشمل السائقين والمندوبين وجميع أنواع الموظفين)</summary>
public class EmployeesController(IMediator mediator) : Controller
{
    public static readonly List<string> AllRoles =
    [
        "محاسب", "مدير", "مدير مبيعات", "مشرف", "سائق",
        "موظف", "كاشير", "مندوب فرع", "مدير نظام", "أمين مستودع"
    ];

    public static readonly List<string> AllRegions =
    [
         "بغداد", "الكاظمية", "المنصور", "الكرادة", "الرصافة", "الكرخ", "الرشيد", "الأعظمية", "الشعب", "الجسر", "المدائن", "أبو غريب", "أخرى"
    ];

    public async Task<IActionResult> Index(string? search, string? employeeType)
    {
        var employees = await mediator.Send(new GetAllEmployeesQuery(search, employeeType));
        ViewBag.Search = search;
        ViewBag.EmployeeType = employeeType;
        return View(employees);
    }

    public IActionResult Create()
    {
        ViewBag.AllRoles = AllRoles;
        ViewBag.AllRegions = AllRegions;
        return View(new CreateEmployeeDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto dto, IFormFile? idImage, IFormFile? photo)
    {
        dto.IdImagePath = await SaveFileAsync(idImage, "employee-ids");
        dto.PhotoPath   = await SaveFileAsync(photo, "employee-photos");

        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = AllRoles;
            ViewBag.AllRegions = AllRegions;
            return View(dto);
        }
        await mediator.Send(new CreateEmployeeCommand(dto));
        await mediator.Send(new LogActivityCommand("إضافة موظف", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم إضافة موظف جديد: {dto.FullName}"));
        TempData["Success"] = "تم إضافة الموظف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var emp = await mediator.Send(new GetEmployeeByIdQuery(id));
        if (emp is null) return NotFound();
        ViewBag.EmployeeId = id;
        ViewBag.AllRoles = AllRoles;
        ViewBag.AllRegions = AllRegions;
        var dto = new UpdateEmployeeDto
        {
            FullName = emp.FullName, Phone = emp.Phone, Address = emp.Address,
            IsActive = emp.IsActive, EmployeeType = emp.EmployeeType,
            SelectedRoles = emp.RolesList, SelectedAreas = emp.AreasList,
            CarNumber = emp.CarNumber, CarType = emp.CarType,
            Region = emp.Region, Branch = emp.Branch,
            IdImagePath = emp.IdImagePath, PhotoPath = emp.PhotoPath
        };
        ViewBag.CurrentIdImage = emp.IdImagePath;
        ViewBag.CurrentPhoto   = emp.PhotoPath;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateEmployeeDto dto, IFormFile? idImage, IFormFile? photo)
    {
        var newId    = await SaveFileAsync(idImage, "employee-ids");
        var newPhoto = await SaveFileAsync(photo, "employee-photos");
        if (newId    != null) dto.IdImagePath = newId;
        if (newPhoto != null) dto.PhotoPath   = newPhoto;

        if (!ModelState.IsValid)
        {
            ViewBag.AllRoles = AllRoles; ViewBag.AllRegions = AllRegions; return View(dto);
        }
        await mediator.Send(new UpdateEmployeeCommand(id, dto));
        await mediator.Send(new LogActivityCommand("تعديل موظف", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم تعديل بيانات الموظف رقم {id}"));
        TempData["Success"] = "تم تعديل بيانات الموظف";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var emp = await mediator.Send(new GetEmployeeByIdQuery(id));
        if (emp is null) return NotFound();
        ViewBag.AllRoles = AllRoles;
        return View(emp);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteEmployeeCommand(id));
        await mediator.Send(new LogActivityCommand("حذف موظف", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم حذف الموظف رقم {id}"));
        TempData["Success"] = "تم حذف الموظف";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Permissions(int id)
    {
        var emp = await mediator.Send(new GetEmployeeByIdQuery(id));
        if (emp is null) return NotFound();
        var perms = await mediator.Send(new GetEmployeePermissionsQuery(id));
        ViewBag.EmployeeId   = id;
        ViewBag.EmployeeName = emp.FullName;
        ViewBag.Pages        = GetSystemPages();
        return View(perms);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePermissions(int employeeId, List<EmployeePermissionDto> permissions)
    {
        await mediator.Send(new SaveEmployeePermissionsCommand(employeeId, permissions));
        TempData["Success"] = "تم حفظ صلاحيات الموظف";
        return RedirectToAction(nameof(Permissions), new { id = employeeId });
    }

    private static List<string> GetSystemPages() =>
    [
        "Dashboard", "Customers", "Employees", "Branches",
        "Categories", "Warehouses", "Products", "Inventory", "Offers",
        "Invoices", "TransferOrders", "Payments", "SalesReturns",
        "Reports", "Debts", "Notifications", "ActivityLogs", "Settings"
    ];

    public async Task<IActionResult> ExportExcel()
    {
        var employees = await mediator.Send(new GetAllEmployeesQuery());
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("الموظفون");
        ws.Cell(1,1).Value="الاسم"; ws.Cell(1,2).Value="الهاتف"; ws.Cell(1,3).Value="النوع";
        ws.Cell(1,4).Value="الأدوار"; ws.Cell(1,5).Value="المنطقة"; ws.Cell(1,6).Value="الفرع"; ws.Cell(1,7).Value="الحالة";
        int row = 2;
        foreach (var e in employees)
        {
            ws.Cell(row,1).Value = e.FullName; ws.Cell(row,2).Value = e.Phone;
            ws.Cell(row,3).Value = TypeLabel(e.EmployeeType); ws.Cell(row,4).Value = e.Roles ?? "-";
            ws.Cell(row,5).Value = e.Region ?? "-"; ws.Cell(row,6).Value = e.Branch ?? "-";
            ws.Cell(row,7).Value = e.IsActive ? "نشط" : "غير نشط";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الموظفون.xlsx");
    }

    private static string TypeLabel(string t) => t switch
    {
        "Individual"     => "فرد",
        "Representative" => "مندوب",
        "Wholesale"      => "جملة",
        _                => t
    };

    private static async Task<string?> SaveFileAsync(IFormFile? file, string folder)
    {
        if (file is null || file.Length == 0) return null;
        var dir = Path.Combine("wwwroot", "uploads", folder);
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(dir, fileName);
        using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);
        return $"/uploads/{folder}/{fileName}";
    }
}
