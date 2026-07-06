using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.Module.Archive.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Application.Interfaces;

public interface IArchiveConfigService
{
    Task<Result<ArchiveConfigDto>> GetAsync();
    Task<Result<ArchiveConfigDto>> UpdateAsync(UpdateArchiveConfigDto dto);
    Task<Result<string[]>> GetSystemDrivesAsync();
    Task<Result<string[]>> GetSubdirectoriesAsync(string path);
}
