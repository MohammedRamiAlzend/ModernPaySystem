using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface ILookUpFiledValuesService
{
    Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetAllAsync();

    Task<Result<PagedList<LookUpFiledValuesDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<LookUpFiledValuesDto>> GetByIdAsync(Guid id);

    Task<Result<LookUpFiledValuesDto>> CreateAsync(CreateLookUpFiledValuesDto lookUpFiledValue);

    Task<Result<LookUpFiledValuesDto>> UpdateAsync(Guid id, UpdateLookUpFiledValuesDto lookUpFiledValue);

    Task<Result<bool>> DeleteAsync(Guid id);

    Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetByLookUpFieldIdAsync(Guid lookUpFieldId);
}
