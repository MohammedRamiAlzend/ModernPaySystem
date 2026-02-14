using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for LookUpField service CRUD operations.
/// </summary>
public interface ILookUpFieldService
{
    /// <summary>
    /// Get all lookup fields.
    /// </summary>
    Task<Result<IEnumerable<LookUpFieldDto>>> GetAllAsync();

    /// <summary>
    /// Get paged lookup fields.
    /// </summary>
    Task<Result<PagedList<LookUpFieldDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get lookup field by id.
    /// </summary>
    Task<Result<LookUpFieldDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Create new lookup field.
    /// </summary>
    Task<Result<LookUpFieldDto>> CreateAsync(CreateLookUpFieldDto lookUpField);

    /// <summary>
    /// Update lookup field.
    /// </summary>
    Task<Result<LookUpFieldDto>> UpdateAsync(Guid id, UpdateLookUpFieldDto lookUpField);

    /// <summary>
    /// Delete lookup field.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Get lookup fields by template ID.
    /// </summary>
    Task<Result<IEnumerable<LookUpFieldDto>>> GetByTemplateIdAsync(Guid templateId);
}