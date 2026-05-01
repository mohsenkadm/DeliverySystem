namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات سجل النشاطات</summary>
public class ActivityLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}

/// <summary>استجابة المصادقة الناجحة تحتوي على التوكن</summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
}

/// <summary>نموذج الاستجابة العامة للـ API</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string MessageAr { get; set; } = string.Empty;
    public string MessageEn { get; set; } = string.Empty;
    public T? Data { get; set; }

    /// <summary>إنشاء استجابة ناجحة</summary>
    public static ApiResponse<T> Ok(T data, string messageAr = "تمت العملية بنجاح", string messageEn = "Operation completed successfully")
        => new() { Success = true, Data = data, MessageAr = messageAr, MessageEn = messageEn };

    /// <summary>إنشاء استجابة خطأ</summary>
    public static ApiResponse<T> Fail(string messageAr, string messageEn)
        => new() { Success = false, MessageAr = messageAr, MessageEn = messageEn };
}
