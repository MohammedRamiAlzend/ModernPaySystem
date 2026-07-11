using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveAuthorizationService(
    ArchiveDbContext dbContext,
    IServiceProvider serviceProvider) : IArchiveAuthorizationService
{
    public async Task<Result<bool>> IsArchiveLeaderAsync(Guid userId, Guid departmentId)
    {
        if (userId == Guid.Empty || departmentId == Guid.Empty)
            return Error.Validation("InvalidInput", "The provided input is invalid.");

        var exists = await dbContext.DepartmentArchiveLeaders
            .AnyAsync(x => x.UserId == userId && x.DepartmentId == departmentId && !x.IsDeleted);

        return exists;
    }

    public async Task<Result<bool>> IsDepartmentHeadAsync(Guid userId, Guid departmentId)
    {
        if (userId == Guid.Empty || departmentId == Guid.Empty)
            return Error.Validation("InvalidInput", "The provided input is invalid.");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var departmentService = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
            var departmentResult = await departmentService.GetByIdAsync(departmentId);
            
            if (departmentResult.IsError || departmentResult.Value == null)
                return false;

            return departmentResult.Value.DepartmentHeadId == userId;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Result<Guid?>> ResolveFolderDepartmentIdAsync(Guid folderId)
    {
        if (folderId == Guid.Empty)
            return Error.Validation("InvalidInput", "The provided input is invalid.");

        var current = await dbContext.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == folderId)
            .Select(x => new { x.DepartmentId, x.ParentId })
            .SingleOrDefaultAsync();

        while (current != null)
        {
            if (current.DepartmentId.HasValue)
                return current.DepartmentId;

            if (!current.ParentId.HasValue)
                return (Guid?)null;

            current = await dbContext.Folders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.Id == current.ParentId.Value)
                .Select(x => new { x.DepartmentId, x.ParentId })
                .SingleOrDefaultAsync();
        }

        return (Guid?)null;
    }

    public async Task<Result<Guid?>> ResolveArchiveRecordDepartmentIdAsync(Guid recordId)
    {
        if (recordId == Guid.Empty)
            return Error.Validation("InvalidInput", "The provided input is invalid.");

        var record = await dbContext.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId)
            .Select(x => new { x.DepartmentId, x.FolderId })
            .SingleOrDefaultAsync();

        if (record == null)
            return Error.NotFound("ArchiveRecordNotFound", "The specified archive record was not found.");

        if (record.DepartmentId.HasValue)
            return record.DepartmentId;

        return await ResolveFolderDepartmentIdAsync(record.FolderId);
    }

    public async Task<Result<List<Guid>>> GetUserArchiveLeaderDepartmentsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return Error.Validation("InvalidInput", "The provided input is invalid.");

        var departments = await dbContext.DepartmentArchiveLeaders
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => x.DepartmentId)
            .ToListAsync();

        return departments;
    }
}
