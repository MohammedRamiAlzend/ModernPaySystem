using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.DTOs;
using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.Module.Transaction.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Application.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<UserDto>>> GetAllAsync();

    Task<Result<PagedList<UserDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<UserDto>> GetByIdAsync(Guid id);

    Task<Result<UserDto>> GetByUsernameAsync(string username);

    Task<Result<UserDto>> CreateAsync(CreateUserDto user);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<IEnumerable<UserDto>>> GetBySubSystemAsync(SubSystem subSystem);

    Task<Result<UserDto>> UpdateAsync(Guid id, CreateUserDto user);

    Task<Result<IEnumerable<TemplateDto>>> GetVisitedTemplatesAsync(Guid userId);

    Task<Result<List<SubSystemDto>>> GetSubSystemsAsync();

    Task<Result<IEnumerable<UserDto>>> GetCurrentDepartmentUsersAsync(Guid currentUserId);
}
