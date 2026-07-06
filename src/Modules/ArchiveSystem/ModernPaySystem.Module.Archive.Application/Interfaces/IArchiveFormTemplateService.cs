using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.Module.Archive.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Application.Interfaces;

public interface IArchiveFormTemplateService
{
    Task<Result<IEnumerable<ArchiveFormTemplateDto>>> GetAllAsync();
    Task<Result<PagedList<ArchiveFormTemplateDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<ArchiveFormTemplateDto>> GetByIdAsync(Guid id);
    Task<Result<ArchiveFormTemplateDto>> GetByNameAsync(string name);
    Task<Result<ArchiveFormTemplateDto>> CreateAsync(CreateDynamicFormTemplateDto dto);
    Task<Result<ArchiveFormTemplateDto>> UpdateAsync(Guid id, UpdateDynamicFormTemplateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
}
