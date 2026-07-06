using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface ILookUpFieldService
{
    Task<Result<IEnumerable<LookUpFieldDto>>> GetAllAsync();

    Task<Result<PagedList<LookUpFieldDto>>> GetPagedAsync(int page, int pageSize);

    Task<Result<LookUpFieldDto>> GetByIdAsync(Guid id);

    Task<Result<LookUpFieldDto>> CreateAsync(CreateLookUpFieldDto lookUpField);

    Task<Result<LookUpFieldDto>> UpdateAsync(Guid id, UpdateLookUpFieldDto lookUpField);

    Task<Result<bool>> DeleteAsync(Guid id);
}
