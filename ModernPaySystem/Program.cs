using Scalar.AspNetCore;
using ModernPaySystem.Infrastructure.Persistence;
using ModernPaySystem.Infrastructure.Persistence.Seeding;
using ModernPaySystem.Infrastructure;
using ModernPaySystem.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services.AddSeeding(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorizationPolicies();

builder.Services.AddInfrastructureServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
