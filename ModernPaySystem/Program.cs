global using static ModernPaySystem.Domain.Commons.Auth.Permission;
using Scalar.AspNetCore;
using ModernPaySystem.Infrastructure.Persistence;
using ModernPaySystem.Infrastructure;
using ModernPaySystem.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add Persistence Services
builder.Services.AddPersistenceServices(builder.Configuration);

// Add Authentication Services
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Authorization Policies
builder.Services.AddAuthorizationPolicies();

// Add Infrastructure Services (Unit of Work, Services, etc.)
builder.Services.AddInfrastructureServices();

// Add API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Add Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
