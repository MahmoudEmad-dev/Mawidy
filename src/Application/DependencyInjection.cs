using Mawidy.Application.Interfaces;
using Mawidy.Application.Services;
using Mawidy.Application.Banks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mawidy.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application-layer services (use cases, MediatR, etc.)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR — scan this assembly for handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Application services
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAdminService, AdminService>();

        // Localization (Banks module)
        services.AddScoped<LocalizationService>();

        return services;
    }
}
