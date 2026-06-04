using FileManager.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Application.Interfaces.TransactionSystemInterfaces;
using ModernPaySystem.Infrastructure.Auth;
using ModernPaySystem.Infrastructure.Auth.Services;
using ModernPaySystem.Infrastructure.Persistence.Interceptors;
using ModernPaySystem.Infrastructure.Options;
using ModernPaySystem.Infrastructure.Services;
using NumberSpelling;
using OcrReader;

namespace ModernPaySystem.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ArchiveRecordFileUploadOptions>(configuration.GetSection("ArchiveRecordFiles"));
        services.Configure<ArchiveRecordZipOptions>(configuration.GetSection("ArchiveRecordZip"));

        // Register File Manager Services
        services.AddFileManager();
        services.AddMemoryCache();

        // Register HTTP Context Accessor
        services.AddHttpContextAccessor();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Authentication Services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Register CRUD Services
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IResponseService, ResponseService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRequestTransactionService, RequestTransactionService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IWebAttachmentService, WebAttachmentService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<IDynamicFormService, DynamicFormService>();
        services.AddScoped<IArchiveFormTemplateService, ArchiveFormTemplateService>();
        services.AddScoped<IArchiveRecordService, ArchiveRecordService>();
        services.AddScoped<IArchiveLeaderService, ArchiveLeaderService>();
        services.AddScoped<IArchiveDeletionWorkflowService, ArchiveDeletionWorkflowService>();
        services.AddScoped<IArchiveAuthorizationService, ArchiveAuthorizationService>();
        services.AddTransient<IHttpContextServiceManager, HttpContextServiceManager>();

        // Register Lookup Field Services
        services.AddScoped<ILookUpFieldService, LookUpFieldService>();
        services.AddScoped<ILookUpFiledValuesService, LookUpFiledValuesService>();

        // Register OCR Service
        services.AddOcrTesseract();
        services.AddScoped<IOcrService, OcrService>();

        // Register Number Spelling Service
        services.AddNumberSpelling();
        services.AddScoped<INumberSpellingWrapperService, NumberSpellingWrapperService>();
        services.AddTransient<AuditInterceptor>();

        // Register Department Service
        services.AddScoped<IDepartmentService, DepartmentService>();

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DepartmentArchiveLeaderAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DepartmentHeadAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, DeleteArchiveRequestHeadAuthorizationHandler>();

        services.AddTransient<IPermissionSeederService>(provider =>
        {
            var applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            var uow = provider.GetRequiredService<IUnitOfWork>();
            return new PermissionSeederService(provider, applicationPartManager, uow);
        });
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }

    /// <summary>
    /// Adds authorization policies for the application.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(ArchiveAuthorizationPolicyExtensions.RequireDepartmentArchiveLeader, policy =>
                policy.RequireAuthenticatedUser().RequireDepartmentArchiveLeader());

            options.AddPolicy(ArchiveAuthorizationPolicyExtensions.RequireDepartmentHead, policy =>
                policy.RequireAuthenticatedUser().RequireDepartmentHead());
        });

        return services;
    }
}
