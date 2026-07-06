using FileManager.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Infrastructure.Options;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Infrastructure.Auth;
using ModernPaySystem.Module.Archive.Infrastructure.Interceptors;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.Module.Archive.Infrastructure.Seeding;
using ModernPaySystem.Module.Archive.Infrastructure.Services;
using ModernPaySystem.Module.Archive.Infrastructure.Services.Qdrant;
using OcrReader;
using SemanticSearchLib.Extensions;
using ModernPaySystem.SharedKernel.Infrastructure;

namespace ModernPaySystem.Module.Archive.Infrastructure;

public static class ArchiveModuleRegistration
{
    public static IServiceCollection AddArchiveModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSharedKernel();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ArchiveDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ArchiveDbContext).Assembly.FullName);
                });
            options.AddInterceptors(serviceProvider.GetRequiredService<ArchiveAuditInterceptor>());
        });

        services.AddScoped<ArchiveAuditInterceptor>();
        services.AddScoped<IArchiveUnitOfWork, ArchiveUnitOfWork>();

        services.AddScoped<IArchiveConfigService, ArchiveConfigService>();
        services.AddScoped<IArchiveConfigSeeder, ArchiveConfigSeeder>();
        services.AddScoped<IArchiveAuthorizationService, ArchiveAuthorizationService>();
        services.AddScoped<IArchiveRecordService, ArchiveRecordService>();
        services.AddScoped<IArchiveDeletionWorkflowService, ArchiveDeletionWorkflowService>();
        services.AddScoped<IArchiveEditWorkflowService, ArchiveEditWorkflowService>();
        services.AddScoped<IArchiveFormTemplateService, ArchiveFormTemplateService>();
        services.AddScoped<IArchiveLeaderService, ArchiveLeaderService>();
        services.AddScoped<IArchiveRecordReportService, ArchiveRecordReportService>();
        services.AddScoped<IArchiveResourceAuthorizationService, ArchiveResourceAuthorizationService>();
        services.AddScoped<IFolderIconService, FolderIconService>();
        services.AddScoped<IFolderService, FolderService>();
        // Options
        services.Configure<ArchiveRecordFileUploadOptions>(
            configuration.GetSection("ArchiveRecordFiles"));
        services.Configure<ArchiveRecordZipOptions>(
            configuration.GetSection("ArchiveRecordZip"));

        services.AddMemoryCache();
        services.AddFileManager();

        // OCR
        services.AddOcrTesseract();
        services.AddScoped<IOcrService, OcrService>();

        // Semantic Search
        services.AddSemanticSearchLib(configuration);
        services.Configure<Options.QdrantOptions>(configuration.GetSection(Options.QdrantOptions.SectionName));
        services.AddSingleton<IQdrantVectorStore, QdrantVectorStore>();
        services.AddScoped<ISemanticSearchService, SemanticSearchService>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(ArchiveAuthorizationPolicyExtensions.RequireDepartmentArchiveLeader, policy =>
                policy.RequireAuthenticatedUser().RequireDepartmentArchiveLeader());

            options.AddPolicy(ArchiveAuthorizationPolicyExtensions.RequireDepartmentHead, policy =>
                policy.RequireAuthenticatedUser().RequireDepartmentHead());
        });

        services.AddScoped<IAuthorizationHandler, DepartmentArchiveLeaderAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DepartmentHeadAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DeleteArchiveRequestHeadAuthorizationHandler>();

        return services;
    }
}
