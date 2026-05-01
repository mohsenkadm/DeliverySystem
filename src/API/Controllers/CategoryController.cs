using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Categories.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

/// <summary>Controller تصنيفات المنتجات</summary>
[ApiController]
[Route("api/categories")]
[Authorize]
[Produces("application/json")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    /// <summary>جلب جميع التصنيفات</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new GetAllCategoriesQuery());
        return Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(result));
    }

    /// <summary>جلب تصنيف بالمعرف</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id));
        if (result is null) return NotFound(ApiResponse<CategoryDto>.Fail("التصنيف غير موجود", "Category not found"));
        return Ok(ApiResponse<CategoryDto>.Ok(result));
    }
}
