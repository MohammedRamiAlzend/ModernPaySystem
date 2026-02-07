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
    Task<Result<IEnumerable<Template>>> GetAllAsync();

    /// <summary>
    /// Get template by id.
    /// </summary>
    Task<Result<Template>> GetByIdAsync(Guid id);

    /// <summary>
    /// Get template by name.
    /// </summary>
    Task<Result<Template>> GetByNameAsync(string name);

    /// <summary>
    /// Create new template.
    /// </summary>
    Task<Result<Template>> CreateAsync(Template template);

    /// <summary>
    /// Update template.
    /// </summary>
    Task<Result<Template>> UpdateAsync(Guid id, Template template);

    /// <summary>
    /// Delete template.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);
}
