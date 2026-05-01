using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Categories.Commands;
using DeliverySystem.Application.Features.Products.Commands;
using DeliverySystem.Application.Features.Warehouses.Commands;
using DeliverySystem.Application.Features.Inventories.Commands;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller إدارة تصنيفات المنتجات</summary>
public class CategoriesController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var cats = await mediator.Send(new GetAllCategoriesQuery(search));
        ViewBag.Search = search;
        return View(cats);
    }

    public IActionResult Create() => View(new CreateCategoryDto());

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await mediator.Send(new CreateCategoryCommand(dto));
        TempData["Success"] = "تم إضافة التصنيف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cat = await mediator.Send(new GetCategoryByIdQuery(id));
        if (cat is null) return NotFound();
        ViewBag.CatId = id;
        return View(new CreateCategoryDto { Name = cat.Name });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateCategoryDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await mediator.Send(new UpdateCategoryCommand(id, dto.Name));
        TempData["Success"] = "تم تعديل التصنيف";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteCategoryCommand(id));
        TempData["Success"] = "تم حذف التصنيف";
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Controller إدارة المستودعات</summary>
public class WarehousesController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(string? search)
        => View(await mediator.Send(new GetAllWarehousesQuery(search)));

    public IActionResult Create() => View(new CreateWarehouseDto());

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWarehouseDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await mediator.Send(new CreateWarehouseCommand(dto));
        TempData["Success"] = "تم إضافة المستودع";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var wh = await mediator.Send(new GetWarehouseByIdQuery(id));
        if (wh is null) return NotFound();
        ViewBag.WhId = id;
        return View(new CreateWarehouseDto { Name = wh.Name, Location = wh.Location });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateWarehouseDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await mediator.Send(new UpdateWarehouseCommand(id, dto));
        TempData["Success"] = "تم تعديل المستودع";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteWarehouseCommand(id));
        TempData["Success"] = "تم حذف المستودع";
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Controller إدارة المنتجات</summary>
public class ProductsController(IMediator mediator) : Controller
{
    public static readonly List<string> CartonTypes = ["صندوق", "كيلوغرام", "غرام", "لتر", "وحدة", "علبة", "كرتون"];

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
        ViewBag.Search = search; ViewBag.CategoryId = categoryId;
        return View(await mediator.Send(new GetAllProductsQuery(search, categoryId)));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
        ViewBag.CartonTypes = CartonTypes;
        return View(new CreateProductDto());
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto dto, IFormFile? image)
    {
        dto.ImagePath = await SaveImageAsync(image) ?? dto.ImagePath;
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
            ViewBag.CartonTypes = CartonTypes;
            return View(dto);
        }
        await mediator.Send(new CreateProductCommand(dto));
        TempData["Success"] = "تم إضافة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id));
        if (product is null) return NotFound();
        ViewBag.ProductId = id;
        ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
        ViewBag.CartonTypes = CartonTypes;
        ViewBag.CurrentImage = product.ImagePath;
        return View(new UpdateProductDto
        {
            Name = product.Name, Description = product.Description,
            WholesalePrice = product.WholesalePrice, RetailPrice = product.RetailPrice,
            DiscountPercentage = product.DiscountPercentage, CartonType = product.CartonType,
            BaseQuantity = product.BaseQuantity, ProductionDate = product.ProductionDate,
            ExpirationDate = product.ExpirationDate, ImagePath = product.ImagePath,
            CategoryId = product.CategoryId
        });
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductDto dto, IFormFile? image)
    {
        var newImg = await SaveImageAsync(image);
        if (newImg != null) dto.ImagePath = newImg;
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await mediator.Send(new GetAllCategoriesQuery());
            ViewBag.CartonTypes = CartonTypes;
            return View(dto);
        }
        await mediator.Send(new UpdateProductCommand(id, dto));
        TempData["Success"] = "تم تعديل المنتج";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteProductCommand(id));
        TempData["Success"] = "تم حذف المنتج";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var products = await mediator.Send(new GetAllProductsQuery());
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("المنتجات");
        ws.Cell(1,1).Value="الاسم"; ws.Cell(1,2).Value="الكود"; ws.Cell(1,3).Value="سعر الجملة";
        ws.Cell(1,4).Value="سعر المفرق"; ws.Cell(1,5).Value="الخصم%"; ws.Cell(1,6).Value="التصنيف";
        int row = 2;
        foreach (var p in products)
        {
            ws.Cell(row,1).Value=p.Name; ws.Cell(row,2).Value=p.Code;
            ws.Cell(row,3).Value=p.WholesalePrice; ws.Cell(row,4).Value=p.RetailPrice;
            ws.Cell(row,5).Value=p.DiscountPercentage; ws.Cell(row,6).Value=p.CategoryName; row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "المنتجات.xlsx");
    }

    private static async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var dir = Path.Combine("wwwroot", "uploads", "products");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        using var fs = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
        await file.CopyToAsync(fs);
        return $"/uploads/products/{fileName}";
    }
}

/// <summary>Controller إدارة المخزون</summary>
public class InventoryController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(int? productId, int? warehouseId)
    {
        ViewBag.Products = await mediator.Send(new GetAllProductsQuery());
        ViewBag.Warehouses = await mediator.Send(new GetAllWarehousesQuery());
        return View(await mediator.Send(new GetAllInventoriesQuery(productId, warehouseId)));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await mediator.Send(new GetAllProductsQuery());
        ViewBag.Warehouses = await mediator.Send(new GetAllWarehousesQuery());
        return View(new CreateInventoryDto());
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInventoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Products = await mediator.Send(new GetAllProductsQuery());
            ViewBag.Warehouses = await mediator.Send(new GetAllWarehousesQuery());
            return View(dto);
        }
        await mediator.Send(new UpsertInventoryCommand(dto));
        TempData["Success"] = "تم تحديث المخزون";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteInventoryCommand(id));
        TempData["Success"] = "تم حذف السجل";
        return RedirectToAction(nameof(Index));
    }
}
