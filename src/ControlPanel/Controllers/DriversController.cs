using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>السائقون تم دمجهم في صفحة الموظفين</summary>
public class DriversController : Controller
{
    public IActionResult Index()    => RedirectToAction("Index",   "Employees");
    public IActionResult Create()   => RedirectToAction("Create",  "Employees");
    public IActionResult Details(int id) => RedirectToAction("Details", "Employees", new { id });
    public IActionResult Edit(int id)    => RedirectToAction("Edit",    "Employees", new { id });
}
