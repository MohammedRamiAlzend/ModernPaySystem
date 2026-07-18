global using System.Threading;
global using System.Threading.Tasks;

namespace ModernPaySystem.Application.Interfaces;

public interface IPermissionSeederService
{
    Task SeedPermissionsAsync(CancellationToken cancellationToken = default);
}
