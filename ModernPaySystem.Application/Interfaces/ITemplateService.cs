namespace ModernPaySystem.Application.Interfaces;

public interface ITemplateService
{
    Task<Result<IEnumerable<TemplateDto>>> GetAllAsync();

    Task<Result<PagedList<TemplateDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<TemplateDto>> GetByIdAsync(Guid id);

    Task<Result<TemplateDto>> GetByNameAsync(string name);

    Task<Result<TemplateDto>> CreateAsync(CreateTemplateDto template);

    Task<Result<TemplateDto>> UpdateAsync(Guid id, UpdateTemplateDto template);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<IEnumerable<TemplateOwnershipDto>>> GetOwnershipsAsync(Guid templateId);
    Task<Result<TemplateOwnershipDto>> AddOwnershipAsync(Guid templateId, Guid departmentId);
    Task<Result<bool>> RemoveOwnershipAsync(Guid templateId, Guid departmentId);

    Task<Result<IEnumerable<UserTemplateOwnershipDto>>> GetUserOwnershipsAsync(Guid templateId);
    Task<Result<UserTemplateOwnershipDto>> AddUserOwnershipAsync(Guid templateId, Guid userId);
    Task<Result<UserTemplateOwnershipDto>> RemoveUserOwnershipAsync(Guid templateId, Guid userId);
}

public class TemplateOwnershipDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}

public class CreateTemplateOwnershipDto
{
    public required Guid DepartmentId { get; set; }
}

public class UserTemplateOwnershipDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
}

public class CreateUserTemplateOwnershipDto
{
    public Guid UserId { get; set; }
}
