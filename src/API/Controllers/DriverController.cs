using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Auth.Commands;
using DeliverySystem.Application.Features.Employees.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/driver")]
[Produces("application/json")]
public class DriverController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    public async Task<IActionResult> Login([FromBody] EmployeeLoginDto dto)
    {
        var result = await mediator.Send(new EmployeeLoginCommand(dto));
        if (result is null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("بيانات الدخول غير صحيحة", "Invalid credentials"));
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Driver,Employee")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), 200)]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(new GetEmployeeByIdQuery(userId));
        if (result is null) return NotFound(ApiResponse<EmployeeDto>.Fail("الموظف غير موجود", "Employee not found"));
        return Ok(ApiResponse<EmployeeDto>.Ok(result));
    }
}
