using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IDynamicFormService
{
    Task<Result<IEnumerable<ArchiveFormTemplateDto>>> GetAllAsync();
    Task<Result<PagedList<ArchiveFormTemplateDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<ArchiveFormTemplateDto>> GetByIdAsync(Guid id);
    Task<Result<ArchiveFormTemplateDto>> GetByNameAsync(string name);
    Task<Result<ArchiveFormTemplateDto>> CreateAsync(CreateDynamicFormTemplateDto dto);
    Task<Result<ArchiveFormTemplateDto>> UpdateAsync(Guid id, UpdateDynamicFormTemplateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
