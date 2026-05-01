using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Auth.Commands;
using DeliverySystem.Application.Features.Admins.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

/// <summary>Controller مصادقة وإدارة المسؤولين</summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary>تسجيل دخول المسؤول</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
    {
        var result = await mediator.Send(new AdminLoginCommand(dto));
        if (result is null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("بيانات الدخول غير صحيحة", "Invalid credentials"));
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>إضافة مسؤول جديد (يتطلب صلاحية Admin)</summary>
    [HttpPost("add-admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminDto>), 200)]
    public async Task<IActionResult> AddAdmin([FromBody] CreateAdminDto dto)
    {
        var result = await mediator.Send(new CreateAdminCommand(dto));
        return Ok(ApiResponse<AdminDto>.Ok(result, "تم إضافة المسؤول بنجاح", "Admin added successfully"));
    }

    /// <summary>جلب قائمة المسؤولين</summary>
    [HttpGet("admins")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminDto>>), 200)]
    public async Task<IActionResult> GetAdmins()
    {
        var result = await mediator.Send(new GetAllAdminsQuery());
        return Ok(ApiResponse<IEnumerable<AdminDto>>.Ok(result));
    }
}
