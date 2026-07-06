using FileManager.Extensions;
using FileManager.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Infrastructure.Interceptors;
using ModernPaySystem.Module.Transaction.Infrastructure.Persistence;
using ModernPaySystem.Module.Transaction.Infrastructure.Services;
using ModernPaySystem.SharedKernel.Infrastructure;

namespace ModernPaySystem.Module.Transaction.Infrastructure;

public static class TransactionModuleRegistration
{
    public static IServiceCollection AddTransactionModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSharedKernel();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TransactionDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(TransactionDbContext).Assembly.FullName);
                });
            options.AddInterceptors(serviceProvider.GetRequiredService<TransactionAuditInterceptor>());
        });

        services.AddScoped<TransactionAuditInterceptor>();
        services.AddScoped<ITransactionUnitOfWork, TransactionUnitOfWork>();

        services.AddScoped<INumberSpellingWrapperService, NumberSpellingWrapperService>();
        services.AddScoped<IRequestAuditService, RequestAuditService>();
        services.AddScoped<ILookUpFieldService, LookUpFieldService>();
        services.AddScoped<ILookUpFiledValuesService, LookUpFiledValuesService>();
        services.AddScoped<IAttachmentService, AttachmentService>();

        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IRequestTransactionService, RequestTransactionService>();
        services.AddScoped<IResponseService, ResponseService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IWebAttachmentService, WebAttachmentService>();
        services.AddMemoryCache();
        services.AddFileManager();

        return services;
    }
}
