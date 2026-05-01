using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeliverySystem.ControlPanel.Filters;

/// <summary>فلتر التحقق من تسجيل الدخول — يُطبَّق على جميع Controllers</summary>
public class RequireLoginFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action     = context.RouteData.Values["action"]?.ToString();

        // السماح بالوصول لصفحة تسجيل الدخول
        if (controller == "Account" && (action == "Login" || action == "AccessDenied"))
            return;

        var adminId = context.HttpContext.Session.GetString("AdminId");
        if (string.IsNullOrEmpty(adminId))
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
        }
    }
}

/// <summary>خاصية تعطيل فلتر تسجيل الدخول لصفحات محددة</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowAnonymousAttribute : Attribute { }
