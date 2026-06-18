using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveConfigService
{
    Task<Result<ArchiveConfigDto>> GetAsync();
    Task<Result<ArchiveConfigDto>> UpdateAsync(UpdateArchiveConfigDto dto);
}