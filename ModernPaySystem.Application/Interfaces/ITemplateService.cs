using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for Template service CRUD operations.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Get all templates.
    /// </summary>
    Task<Result<IEnumerable<TemplateDto>>> GetAllAsync();

    /// <summary>
    /// Get paged templates.
    /// </summary>
    Task<Result<PagedList<TemplateDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get template by id.
    /// </summary>
    Task<Result<TemplateDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get template by name.
    /// </summary>
    Task<Result<TemplateDto>> GetByNameAsync(string name);

    /// <summary>
    /// Create new template.
    /// </summary>
    Task<Result<TemplateDto>> CreateAsync(CreateTemplateDto template);

    /// <summary>
    /// Update template.
    /// </summary>
    Task<Result<TemplateDto>> UpdateAsync(Guid id, UpdateTemplateDto template);

    /// <summary>
    /// Delete template.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
