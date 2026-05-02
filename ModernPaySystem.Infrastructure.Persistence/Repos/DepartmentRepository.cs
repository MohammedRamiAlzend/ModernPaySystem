using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Repos;

/// <summary>
/// Repository implementation for Department entity
/// </summary>
public class DepartmentRepository(AppDbContext dbContext) 
    : RepositoryBase<Department, Guid>(dbContext, null!, null!), IDepartmentRepository
{
    public async Task<Result<List<Department>>> GetRootDepartmentsAsync()
    {
        try
        {
            var roots = await dbcontext.Set<Department>()
                .Where(d => d.ParentDepartmentId == null)
                .OrderBy(d => d.Name)
                .ToListAsync();
            
            return roots;
        }
        catch (Exception ex)
        {
            return new Error("DB_ERROR", ex.Message, Domain.Commons.ErrorKind.Failure);
        }
    }

    public async Task<Result<List<Department>>> GetChildrenAsync(Guid parentId)
    {
        try
        {
            var children = await dbcontext.Set<Department>()
                .Where(d => d.ParentDepartmentId == parentId)
                .OrderBy(d => d.Name)
                .ToListAsync();
            
            return children;
        }
        catch (Exception ex)
        {
            return new Error("DB_ERROR", ex.Message, Domain.Commons.ErrorKind.Failure);
        }
    }

    public async Task<Result<List<Department>>> GetPathToRootAsync(Guid departmentId)
    {
        try
        {
            var path = new List<Department>();
            var current = await dbcontext.Set<Department>()
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            while (current != null)
            {
                path.Add(current);
                
                if (current.ParentDepartmentId == null)
                    break;
                    
                current = await dbcontext.Set<Department>()
                    .FirstOrDefaultAsync(d => d.Id == current.ParentDepartmentId);
            }

            path.Reverse();
            return path;
        }
        catch (Exception ex)
        {
            return new Error("DB_ERROR", ex.Message, Domain.Commons.ErrorKind.Failure);
        }
    }

    public async Task<Result<List<Department>>> GetAllDescendantsAsync(Guid departmentId)
    {
        try
        {
            var descendants = new List<Department>();
            await GetDescendantsRecursive(departmentId, descendants);
            return descendants;
        }
        catch (Exception ex)
        {
            return new Error("DB_ERROR", ex.Message, Domain.Commons.ErrorKind.Failure);
        }
    }

    public async Task<bool> HasChildrenAsync(Guid departmentId)
    {
        return await dbcontext.Set<Department>()
            .AnyAsync(d => d.ParentDepartmentId == departmentId);
    }

    public async Task<bool> WouldCreateCircularReferenceAsync(Guid departmentId, Guid parentDepartmentId)
    {
        // A department cannot be its own parent
        if (departmentId == parentDepartmentId)
            return true;

        // Check if the proposed parent is a descendant of the department
        // (which would create a circular reference)
        var descendants = await GetAllDescendantsAsync(departmentId);
        return descendants.Value?.Any(d => d.Id == parentDepartmentId) ?? false;
    }

    private async Task GetDescendantsRecursive(Guid parentId, List<Department> descendants)
    {
        var children = await GetChildrenAsync(parentId);
        if (children.IsError || children.Value == null)
            return;

        foreach (var child in children.Value)
        {
            descendants.Add(child);
            await GetDescendantsRecursive(child.Id, descendants);
        }
    }
}
