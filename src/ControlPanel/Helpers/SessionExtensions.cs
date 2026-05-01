namespace DeliverySystem.ControlPanel.Helpers;

/// <summary>امتدادات للوصول لبيانات الجلسة في لوحة التحكم</summary>
public static class SessionExtensions
{
    public static string GetAdminFullName(this IHttpContextAccessor accessor)
        => accessor.HttpContext?.Session.GetString("AdminFullName") ?? "المسؤول";

    public static string GetAdminUsername(this IHttpContextAccessor accessor)
        => accessor.HttpContext?.Session.GetString("AdminUsername") ?? "";

    public static int GetAdminId(this IHttpContextAccessor accessor)
        => int.TryParse(accessor.HttpContext?.Session.GetString("AdminId"), out var id) ? id : 0;
}
