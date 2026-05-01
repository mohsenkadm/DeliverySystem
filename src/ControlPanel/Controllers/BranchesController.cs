using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Branches.Commands;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

public class BranchesController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var branches = await mediator.Send(new GetAllBranchesQuery(search, isActive));
        ViewBag.Search   = search;
        ViewBag.IsActive = isActive;
        return View(branches);
    }

    public IActionResult Create() => View(new CreateBranchDto());

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBranchDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await mediator.Send(new CreateBranchCommand(dto));
        TempData["Success"] = "تم إضافة الفرع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await mediator.Send(new GetBranchByIdQuery(id));
        if (branch is null) return NotFound();
        ViewBag.BranchId = id;
        return View(new UpdateBranchDto
        {
            Name = branch.Name, Address = branch.Address,
            Phone = branch.Phone, Region = branch.Region, IsActive = branch.IsActive
        });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateBranchDto dto)
    {
        if (!ModelState.IsValid) { ViewBag.BranchId = id; return View(dto); }
        var ok = await mediator.Send(new UpdateBranchCommand(id, dto));
        if (!ok) return NotFound();
        TempData["Success"] = "تم تحديث الفرع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var branch = await mediator.Send(new GetBranchByIdQuery(id));
        if (branch is null) return NotFound();
        return View(branch);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await mediator.Send(new DeleteBranchCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "تم حذف الفرع بنجاح" : "حدث خطأ أثناء الحذف";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var branches = await mediator.Send(new GetAllBranchesQuery());
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("الفروع");
        ws.Cell(1,1).Value="اسم الفرع"; ws.Cell(1,2).Value="المنطقة";
        ws.Cell(1,3).Value="الهاتف"; ws.Cell(1,4).Value="العنوان";
        ws.Cell(1,5).Value="العملاء"; ws.Cell(1,6).Value="الموظفون";
        ws.Cell(1,7).Value="المستودعات"; ws.Cell(1,8).Value="الحالة";
        int row = 2;
        foreach (var b in branches)
        {
            ws.Cell(row,1).Value = b.Name;    ws.Cell(row,2).Value = b.Region ?? "-";
            ws.Cell(row,3).Value = b.Phone ?? "-"; ws.Cell(row,4).Value = b.Address ?? "-";
            ws.Cell(row,5).Value = b.CustomerCount; ws.Cell(row,6).Value = b.EmployeeCount;
            ws.Cell(row,7).Value = b.WarehouseCount; ws.Cell(row,8).Value = b.IsActive ? "نشط" : "غير نشط";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الفروع.xlsx");
    }
}
