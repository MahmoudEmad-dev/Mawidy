using Mawidy.Application;
using Mawidy.Infrastructure;
using Mawidy.Infrastructure.Persistence;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// ── Application & Infrastructure DI ─────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ── JWT Authentication ──────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
})
.AddCookie("MvcCookies", options =>
{
    options.LoginPath = "/Banks/Home/Login";
    options.AccessDeniedPath = "/Banks/Home/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.Name = ".MvcAuth";
})
.AddCookie("HospitalCookies", options =>
{
    options.LoginPath = "/Hospitals/HospitalAuth/Login";
    options.Cookie.Name = ".HospitalAuth";
    options.AccessDeniedPath = "/Hospitals/HospitalAuth/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// ── CORS ────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://127.0.0.1:5500", "http://localhost:5500",  // main frontend dev server
                "http://localhost:5154",                             // main backend (self)
                "http://localhost:5281",                             // Banks
                "http://localhost:5216"                              // Healthcare
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ── MVC, SignalR, Swagger ───────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mawidy API",
        Version = "v1",
        Description = "API للسجل المدني الرقمي - رواد مصر الرقمية"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "ادخل: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Seed Roles, Admin & Test Users ──────────────────────────────────────
await SeedDatabaseAsync(app);

// ── Middleware Pipeline ─────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Static files: configurable frontend path with relative default
var frontendPath = builder.Configuration["FrontendPath"]
    ?? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "Mawidy-frontend"));

if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });

    var fileServerOptions = new FileServerOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = "",
        EnableDefaultFiles = true
    };
    fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
    fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("index.html");
    app.UseFileServer(fileServerOptions);
}

// Backend wwwroot → serves CSS/JS for Razor views (~/css/site.css, ~/js/branches.js …)
app.UseStaticFiles();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Backward-compatible redirects for old Maw3dyCare URLs (before Areas consolidation)
app.MapGet("/Maw3dyCare/{action}/{id?}", (string action, string? id) =>
    Results.Redirect(id != null ? $"/Hospitals/Maw3dyCare/{action}/{id}" : $"/Hospitals/Maw3dyCare/{action}"));
app.MapGet("/HospitalDashboard/{action}/{id?}", (string action, string? id) =>
    Results.Redirect(id != null ? $"/Hospitals/HospitalDashboard/{action}/{id}" : $"/Hospitals/HospitalDashboard/{action}"));
app.MapGet("/HospitalAuth/{action}/{id?}", (string action, string? id) =>
    Results.Redirect(id != null ? $"/Hospitals/HospitalAuth/{action}/{id}" : $"/Hospitals/HospitalAuth/{action}"));
app.MapGet("/HospitalDashboard", () => Results.Redirect("/Hospitals/HospitalDashboard/Index"));
app.MapGet("/Maw3dyCare", () => Results.Redirect("/Hospitals/Maw3dyCare/Landing"));
app.MapGet("/Maw3dyCare/Hospitals", () => Results.Redirect("/Hospitals/Maw3dyCare/Hospitals"));

// Areas (Banks & Hospitals MVC) - BEFORE default
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// REST API controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<Mawidy.API.Hubs.Banks.BookingHub>("/hubs/booking");
app.MapHub<Mawidy.API.Hubs.Hospitals.ReservationHub>("/hubs/reservation");

app.Run();

// ═════════════════════════════════════════════════════════════════════════
// Local helper: database seeding
// ═════════════════════════════════════════════════════════════════════════
static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(context);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { Roles.Admin, Roles.BranchAdmin, Roles.Citizen };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    await EnsureUserAsync(userManager, new ApplicationUser
    {
        FirstName = "المدير",
        LastName = "العام",
        NationalId = "00000000000000",
        UserName = "admin@civil.com",
        Email = "admin@civil.com",
        PhoneNumber = "01000000000",
        EmailConfirmed = true,
        DateOfBirth = new DateTime(1990, 1, 1)
    }, "Test@1234", Roles.Admin);

    await EnsureUserAsync(userManager, new ApplicationUser
    {
        FirstName = "Test",
        LastName = "User",
        NationalId = "11111111111111",
        UserName = "test@mawidy.com",
        Email = "test@mawidy.com",
        PhoneNumber = "01111111111",
        EmailConfirmed = true,
        DateOfBirth = new DateTime(1995, 1, 1)
    }, "Test@1234", Roles.Citizen);
}

static async Task EnsureUserAsync(
    UserManager<ApplicationUser> userManager,
    ApplicationUser template,
    string password,
    string role)
{
    var existing = await userManager.FindByEmailAsync(template.Email!);
    if (existing != null) return;

    await userManager.CreateAsync(template, password);
    await userManager.AddToRoleAsync(template, role);
}
