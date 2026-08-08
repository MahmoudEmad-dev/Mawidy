using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Infrastructure.Persistence.Repositories;
using Mawidy.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mawidy.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure-layer services (EF Core, Identity, repositories, external services).
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("MyConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AppDbContext>());

        // ── Identity ────────────────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = false;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ── Repositories ────────────────────────────────────────────────────
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IComplaintRepository, ComplaintRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();

        // ── External / Infrastructure services ──────────────────────────────
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IQRService, QRService>();
        services.AddScoped<IPdfReportService, PdfReportService>();
        services.AddScoped<IPeakTimeService, PeakTimeService>();
        services.AddScoped<IAppointmentAvailabilityService, AppointmentAvailabilityService>();

        // ── Background services ─────────────────────────────────────────────
        services.AddHostedService<ReminderService>();
        services.AddHostedService<AppointmentCompletionService>();

        return services;
    }
}
