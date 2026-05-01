using Microsoft.AspNetCore.SignalR;

namespace DeliverySystem.Infrastructure.Hubs;

/// <summary>Hub الإشعارات الفورية عبر SignalR للوحة التحكم</summary>
public class NotificationHub : Hub
{
    /// <summary>إرسال إشعار فوري لجميع المتصلين</summary>
    public async Task SendToAll(string title, string body)
        => await Clients.All.SendAsync("ReceiveNotification", title, body);

    /// <summary>إرسال إشعار لمجموعة محددة (Admin, Driver, إلخ)</summary>
    public async Task SendToGroup(string groupName, string title, string body)
        => await Clients.Group(groupName).SendAsync("ReceiveNotification", title, body);

    /// <summary>الانضمام لمجموعة إشعارات</summary>
    public async Task JoinGroup(string groupName)
        => await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
}
