global using ModernPaySystem.Infrastructure;
global using ModernPaySystem.Infrastructure.Auth;
global using ModernPaySystem.Infrastructure.Persistence;
global using ModernPaySystem.Infrastructure.Persistence.Seeding;
global using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Fatal()
    .WriteTo.File("logs/bootstrap-fatal.log", rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{


    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));
    builder.Logging.ClearProviders();
    builder.Services.AddPersistenceServices(builder.Configuration);

    builder.Services.AddSeeding(builder.Configuration);

    builder.Services.AddJwtAuthentication(builder.Configuration);

    builder.Services.AddAuthorizationPolicies();

    builder.Services.AddInfrastructureServices();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
    });
    builder.Services.AddControllers();

    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    });


    var app = builder.Build();

    if (builder.Configuration.GetValue<bool>("Seeding:Enabled"))
    {
        using var scope = app.Services.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<ISeederOrchestrator>();
        await orchestrator.SeedDatabaseAsync();
    }

    using (var scope = app.Services.CreateScope())
    {
        var permissionSeederService = scope.ServiceProvider.GetRequiredService<IPermissionSeederService>();
        await permissionSeederService.SeedPermissionsAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    //app.UseHttpsRedirection();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.GetLevel = (httpContext, _, ex) =>
        {
            if (ex is not null)
                return Serilog.Events.LogEventLevel.Error;

            var statusCode = (int?)httpContext.Response?.StatusCode;
            return statusCode >= 500
                ? Serilog.Events.LogEventLevel.Error
                : statusCode >= 400
                    ? Serilog.Events.LogEventLevel.Warning
                    : Serilog.Events.LogEventLevel.Information;
        };
    });
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}