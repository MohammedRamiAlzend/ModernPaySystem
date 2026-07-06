using System.Linq.Expressions;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.SharedKernel.Application.Repos;

public interface ISharedReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
}
