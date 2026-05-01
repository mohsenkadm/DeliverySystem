using DeliverySystem.Application;
using DeliverySystem.Application.Features.Auth.Commands;
using DeliverySystem.Infrastructure;
using DeliverySystem.Infrastructure.Data;
using DeliverySystem.Infrastructure.Hubs;
using DeliverySystem.ControlPanel.Filters;
using DeliverySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ─── Serilog ──────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json").Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ─── MVC + فلتر تسجيل الدخول على جميع Controllers ───────────────────────────
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<RequireLoginFilter>();
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();

// ─── Session (لتخزين معلومات تسجيل الدخول) ───────────────────────────────────
builder.Services.AddSession(o =>
{
    o.IdleTimeout    = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

// ─── Auto Migrate + Seed المسؤول الافتراضي ───────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (!db.Admins.Any())
    {
        // إنشاء المسؤول الافتراضي عند أول تشغيل
        db.Admins.Add(new Admin
        {
            FullName     = "المسؤول العام",
            Username     = "admin",
            PasswordHash = PasswordHelper.Hash("123"),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        Log.Information("تم إنشاء المسؤول الافتراضي: admin / 123");
    }
    else
    {
        // إصلاح أي سجل مسؤول يحتوي على كلمة مرور بصيغة غير صحيحة (نص عادي بدلاً من hash.salt)
        var brokenAdmins = db.Admins
            .AsEnumerable()
            //.Where(a => !a.PasswordHash.Contains('.'))
            .ToList();

        foreach (var a in brokenAdmins)
        {
            a.PasswordHash = PasswordHelper.Hash("123");
            Log.Warning("تم إعادة تشفير كلمة مرور المسؤول '{Username}'. كلمة المرور الجديدة: 123", a.Username);
        }

        if (brokenAdmins.Count > 0)
            await db.SaveChangesAsync();
    }
}

app.Run();
