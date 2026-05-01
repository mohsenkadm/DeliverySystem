using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Admins.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller إدارة المسؤولين وصلاحياتهم</summary>
public class AdminsController(IMediator mediator) : Controller
{
    // ─── قائمة المسؤولين ───────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        var admins = await mediator.Send(new GetAllAdminsQuery());
        return View(admins);
    }

    // ─── إضافة مسؤول جديد ─────────────────────────────────────────────────────
    public IActionResult Create() => View(new CreateAdminDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAdminDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await mediator.Send(new CreateAdminCommand(dto));
        TempData["Success"] = "تم إضافة المسؤول بنجاح";
        return RedirectToAction(nameof(Index));
    }

    // ─── عرض تفاصيل مسؤول ──────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var admins = await mediator.Send(new GetAllAdminsQuery());
        var admin  = admins.FirstOrDefault(a => a.Id == id);
        if (admin is null) return NotFound();
        return View(admin);
    }

    // ─── إدارة صلاحيات مسؤول ──────────────────────────────────────────────────
    public async Task<IActionResult> Permissions(int id)
    {
        var admins = await mediator.Send(new GetAllAdminsQuery());
        var admin  = admins.FirstOrDefault(a => a.Id == id);
        if (admin is null) return NotFound();
        ViewBag.AdminId   = id;
        ViewBag.AdminName = admin.FullName;

        // الصفحات المتاحة في النظام
        ViewBag.Pages = GetSystemPages();
        return View(admin.Permissions);
    }

    // ─── حفظ الصلاحيات ────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePermissions(int adminId, List<AdminPermissionDto> permissions)
    {
        await mediator.Send(new SaveAdminPermissionsCommand(adminId, permissions));
        TempData["Success"] = "تم حفظ الصلاحيات بنجاح";
        return RedirectToAction(nameof(Permissions), new { id = adminId });
    }

    // ─── تفعيل / تعطيل مسؤول ─────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await mediator.Send(new ToggleAdminActiveCommand(id));
        TempData["Success"] = "تم تحديث حالة المسؤول";
        return RedirectToAction(nameof(Index));
    }

    // ─── حذف مسؤول ────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // منع حذف المسؤول الحالي
        var currentAdminId = HttpContext.Session.GetString("AdminId");
        if (currentAdminId == id.ToString())
        {
            TempData["Error"] = "لا يمكن حذف حسابك الحالي";
            return RedirectToAction(nameof(Index));
        }
        await mediator.Send(new DeleteAdminCommand(id));
        TempData["Success"] = "تم حذف المسؤول";
        return RedirectToAction(nameof(Index));
    }

    // ─── قائمة صفحات النظام ───────────────────────────────────────────────────
    private static List<string> GetSystemPages() =>
    [
        "Dashboard", "Customers", "Representatives", "Drivers",
        "Categories", "Warehouses", "Products", "Inventory",
        "Invoices", "Debts", "Reports", "ActivityLogs", "Admins"
    ];
}
