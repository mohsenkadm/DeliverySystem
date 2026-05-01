using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Products.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

/// <summary>Controller المنتجات (القراءة متاحة، الكتابة من لوحة التحكم)</summary>
[ApiController]
[Route("api/products")]
[Authorize]
[Produces("application/json")]
public class ProductController(IMediator mediator) : ControllerBase
{
    /// <summary>جلب جميع المنتجات</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? categoryId)
    {
        var result = await mediator.Send(new GetAllProductsQuery(search, categoryId));
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(result));
    }

    /// <summary>جلب منتج بالمعرف</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id));
        if (result is null) return NotFound(ApiResponse<ProductDto>.Fail("المنتج غير موجود", "Product not found"));
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }
}
