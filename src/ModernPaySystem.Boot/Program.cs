using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using ModernPaySystem.Module.Archive.Api.Controllers;
using Serilog;
using Serilog.Events;
using ModernPaySystem.Module.Archive.Infrastructure;
using ModernPaySystem.Module.Identity.Api.Controllers;
using ModernPaySystem.Boot;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Identity.Infrastructure;
using ModernPaySystem.Module.Identity.Infrastructure.Services;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.Module.Identity.Infrastructure.Seeding;
using ModernPaySystem.Module.Transaction.Api.Controllers;
using ModernPaySystem.Module.Transaction.Infrastructure;
using ModernPaySystem.SharedKernel.Infrastructure;
using NumberSpelling;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10_737_418_240; // 10 GB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 10_737_418_240;
});

builder.Services.AddControllers()
    .AddApplicationPart(typeof(RequestsController).Assembly)
    .AddApplicationPart(typeof(ArchiveRecordsController).Assembly)
    .AddApplicationPart(typeof(AuthController).Assembly);

builder.Services.AddOpenApi();

// Serilog structured logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
                 .ReadFrom.Services(services);
});
builder.Logging.ClearProviders();

// Shared infrastructure
builder.Services.AddSharedKernel();

builder.Services.AddNumberSpelling();

var transactionEnabled = builder.Configuration.GetValue<bool>("Modules:TransactionSystem:Enabled");
if (transactionEnabled)
{
    builder.Services.AddTransactionModule(builder.Configuration);
}

var archiveEnabled = builder.Configuration.GetValue<bool>("Modules:Archive:Enabled");
if (archiveEnabled)
{
    builder.Services.AddArchiveModule(builder.Configuration);
}

var identityEnabled = builder.Configuration.GetValue<bool>("Modules:Identity:Enabled");
if (identityEnabled)
{
    builder.Services.AddIdentityModule(builder.Configuration);
}

// Register DepartmentService as the single source of truth (SharedKernel interface, Identity implementation)
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ModernPaySystem.Module.Identity.Infrastructure.Persistence.IdentityDbContext>()
    .AddDbContextCheck<ModernPaySystem.Module.Transaction.Infrastructure.Persistence.TransactionDbContext>()
    .AddDbContextCheck<ModernPaySystem.Module.Archive.Infrastructure.Persistence.ArchiveDbContext>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(
    jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        corsBuilder =>
        {
            corsBuilder
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

var app = builder.Build();

if (identityEnabled && builder.Configuration.GetValue<bool>("Seeding:Enabled"))
{
    using var scope = app.Services.CreateScope();
    var orchestrator = scope.ServiceProvider.GetRequiredService<ISeederOrchestrator>();
    await orchestrator.SeedDatabaseAsync();

    var permissionSeederService = scope.ServiceProvider.GetRequiredService<IPermissionSeederService>();
    await permissionSeederService.SeedPermissionsAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Serilog request logging
app.UseSerilogRequestLogging(opts =>
{
    opts.GetLevel = (httpContext, _, ex) =>
    {
        if (ex is not null)
            return LogEventLevel.Error;

        var statusCode = httpContext.Response?.StatusCode;
        return statusCode >= 500
            ? LogEventLevel.Error
            : statusCode >= 400
                ? LogEventLevel.Warning
                : LogEventLevel.Information;
    };
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PermissionMiddleware>();
app.MapControllers();
app.MapHealthChecks("/healthz");
app.Run();
