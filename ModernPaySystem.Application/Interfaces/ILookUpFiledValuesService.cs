namespace ModernPaySystem.Application.Interfaces;

/// <summary>
/// Interface for LookUpFiledValues service CRUD operations.
/// </summary>
public interface ILookUpFiledValuesService
{
    /// <summary>
    /// Get all lookup field values.
    /// </summary>
    Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetAllAsync();

    /// <summary>
    /// Get paged lookup field values.
    /// </summary>
    Task<Result<PagedList<LookUpFiledValuesDto>>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Get lookup field value by id.
    /// </summary>
    Task<Result<LookUpFiledValuesDto>> GetByIdAsync(Guid id);

    /// <summary>
    /// Create new lookup field value.
    /// </summary>
    Task<Result<LookUpFiledValuesDto>> CreateAsync(CreateLookUpFiledValuesDto lookUpFiledValue);

    /// <summary>
    /// Update lookup field value.
    /// </summary>
    Task<Result<LookUpFiledValuesDto>> UpdateAsync(Guid id, UpdateLookUpFiledValuesDto lookUpFiledValue);

    /// <summary>
    /// Delete lookup field value.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Get lookup field values by lookup field ID.
    /// </summary>
    Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetByLookUpFieldIdAsync(Guid lookUpFieldId);
}
