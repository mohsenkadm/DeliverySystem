namespace DeliverySystem.Domain.Interfaces;

/// <summary>واجهة خدمة JWT للمصادقة وتوليد التوكن</summary>
public interface IJwtService
{
    /// <summary>توليد توكن JWT للمستخدم</summary>
    string GenerateToken(int userId, string username, string role);

    /// <summary>استخراج معرف المستخدم من التوكن</summary>
    int? GetUserIdFromToken(string token);

    /// <summary>استخراج دور المستخدم من التوكن</summary>
    string? GetRoleFromToken(string token);
}
