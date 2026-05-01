using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Auth.Commands;
using DeliverySystem.Application.Features.Customers.Commands;
using DeliverySystem.Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

/// <summary>Controller مصادقة العملاء (تسجيل + دخول + الملف الشخصي)</summary>
[ApiController]
[Route("api/customer")]
[Produces("application/json")]
public class CustomerAuthController(IMediator mediator) : ControllerBase
{
    /// <summary>تسجيل حساب عميل جديد (يحتاج موافقة أدمن)</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), 200)]
    public async Task<IActionResult> Register([FromBody] CreateCustomerDto dto)
    {
        var result = await mediator.Send(new CreateCustomerCommand(dto, IsApproved: false));
        return Ok(ApiResponse<CustomerDto>.Ok(result, "تم التسجيل بنجاح، في انتظار موافقة المدير", "Registered successfully, awaiting admin approval"));
    }

    /// <summary>تسجيل عميل بواسطة مندوب (موافق تلقائياً)</summary>
    [HttpPost("register-by-rep")]
    [Authorize(Roles = "Representative")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), 200)]
    public async Task<IActionResult> RegisterByRep([FromBody] CreateCustomerDto dto)
    {
        var result = await mediator.Send(new CreateCustomerCommand(dto, IsApproved: true));
        return Ok(ApiResponse<CustomerDto>.Ok(result, "تم تسجيل العميل بنجاح", "Customer registered successfully"));
    }

    /// <summary>تسجيل دخول العميل</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    public async Task<IActionResult> Login([FromBody] CustomerLoginDto dto)
    {
        var result = await mediator.Send(new CustomerLoginCommand(dto));
        if (result is null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("بيانات الدخول غير صحيحة أو الحساب غير مفعل", "Invalid credentials or account not approved"));
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    /// <summary>جلب الملف الشخصي للعميل الحالي</summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), 200)]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(new GetCustomerByIdQuery(userId));
        if (result is null) return NotFound(ApiResponse<CustomerDto>.Fail("العميل غير موجود", "Customer not found"));
        return Ok(ApiResponse<CustomerDto>.Ok(result));
    }
}
