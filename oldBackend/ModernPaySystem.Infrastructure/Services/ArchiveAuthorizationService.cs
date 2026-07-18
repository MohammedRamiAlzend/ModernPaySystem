using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveAuthorizationService(IUnitOfWork unitOfWork, ILogger<ArchiveAuthorizationService> logger) : IArchiveAuthorizationService
{
    public async Task<Result<bool>> IsArchiveLeaderAsync(Guid userId, Guid departmentId)
    {
        if (userId == Guid.Empty || departmentId == Guid.Empty)
        {
            return ApplicationErrors.InvalidInput;
        }

        var exists = await unitOfWork.DepartmentArchiveLeaders.AnyAsync(x => x.UserId == userId && x.DepartmentId == departmentId && !x.IsDeleted);
        return exists;
    }

    public async Task<Result<bool>> IsDepartmentHeadAsync(Guid userId, Guid departmentId)
    {
        if (userId == Guid.Empty || departmentId == Guid.Empty)
        {
            return ApplicationErrors.InvalidInput;
        }

        var exists = await unitOfWork.Departments.AnyAsync(x => x.Id == departmentId && x.DepartmentHeadId == userId);
        return exists;
    }

    public async Task<Result<Guid?>> ResolveFolderDepartmentIdAsync(Guid folderId)
    {
        if (folderId == Guid.Empty)
        {
            return ApplicationErrors.InvalidInput;
        }

        var current = await unitOfWork.Context.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == folderId)
            .Select(x => new { x.DepartmentId, x.ParentId })
            .SingleOrDefaultAsync();

        while (current != null)
        {
            if (current.DepartmentId.HasValue)
            {
                return current.DepartmentId;
            }

            if (!current.ParentId.HasValue)
            {
                return (Guid?)null;
            }

            current = await unitOfWork.Context.Folders
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
        {
            return ApplicationErrors.InvalidInput;
        }

        var record = await unitOfWork.Context.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId)
            .Select(x => new { x.DepartmentId, x.FolderId })
            .SingleOrDefaultAsync();

        if (record == null)
        {
            return ApplicationErrors.ArchiveRecordNotFound;
        }

        if (record.DepartmentId.HasValue)
        {
            return record.DepartmentId;
        }

        return await ResolveFolderDepartmentIdAsync(record.FolderId);
    }

    public async Task<Result<List<Guid>>> GetUserArchiveLeaderDepartmentsAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return ApplicationErrors.InvalidInput;
        }

        var departments = await unitOfWork.Context.DepartmentArchiveLeaders
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => x.DepartmentId)
            .ToListAsync();

        return departments;
    }
}