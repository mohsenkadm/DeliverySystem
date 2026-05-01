using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DeliverySystem.Infrastructure.Services;

/// <summary>خدمة الإشعارات عبر OneSignal وحفظها في قاعدة البيانات</summary>
public class NotificationService(
    IUnitOfWork uow,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly string _appId = config["OneSignal:AppId"] ?? string.Empty;
    private readonly string _apiKey = config["OneSignal:ApiKey"] ?? string.Empty;
    private readonly string _apiUrl = config["OneSignal:ApiUrl"] ?? "https://onesignal.com/api/v1/notifications";

    /// <summary>إرسال إشعار وحفظه في قاعدة البيانات</summary>
    public async Task SendNotificationAsync(string title, string body, NotificationTarget target, int? targetUserId = null)
    {
        var notification = new Notification
        {
            Title = title, Body = body, Target = target,
            TargetUserId = targetUserId, IsRead = false, CreatedAt = DateTime.UtcNow
        };
        await uow.Notifications.AddAsync(notification);
        await uow.SaveChangesAsync();
        await SendOneSignalNotificationAsync(title, body);
    }

    /// <summary>إرسال إشعار عبر OneSignal REST API</summary>
    public async Task SendOneSignalNotificationAsync(string title, string body, string? playerId = null)
    {
        if (string.IsNullOrWhiteSpace(_appId) || string.IsNullOrWhiteSpace(_apiKey)) return;
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {_apiKey}");
            var payload = new
            {
                app_id = _appId,
                included_segments = playerId is null ? new[] { "All" } : null,
                include_player_ids = playerId is not null ? new[] { playerId } : null,
                headings = new { en = title, ar = title },
                contents = new { en = body, ar = body }
            };
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_apiUrl, content);
            if (!response.IsSuccessStatusCode)
                logger.LogWarning("OneSignal notification failed: {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending OneSignal notification");
        }
    }
}
