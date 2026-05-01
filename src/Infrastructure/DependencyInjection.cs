using DeliverySystem.Domain.Interfaces;
using DeliverySystem.Infrastructure.Data;
using DeliverySystem.Infrastructure.Repositories;
using DeliverySystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeliverySystem.Infrastructure;

/// <summary>تسجيل خدمات طبقة Infrastructure في حاوية الخدمات</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // تسجيل قاعدة البيانات
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // تسجيل وحدة العمل والمستودعات
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // تسجيل الخدمات
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<INotificationService, NotificationService>();

        // تسجيل HttpClient لخدمة OneSignal
        services.AddHttpClient();

        return services;
    }
}
