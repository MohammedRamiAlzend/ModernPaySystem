global using ModernPaySystem.Infrastructure;
global using ModernPaySystem.Infrastructure.Auth;
global using ModernPaySystem.Infrastructure.Persistence;
global using ModernPaySystem.Infrastructure.Persistence.Seeding;
global using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG == false
builder.WebHost.UseUrls("http://0.0.0.0:7010/");
#endif
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

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapFallbackToFile("index.html");
app.UseStaticFiles();
app.MapControllers();
app.Run();
