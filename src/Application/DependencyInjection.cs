using DeliverySystem.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DeliverySystem.Application;

/// <summary>تسجيل خدمات طبقة Application في حاوية الخدمات</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // تسجيل MediatR - CQRS handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // تسجيل FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
