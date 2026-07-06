namespace ModernPaySystem.Module.Identity.Application.Interfaces;

public interface IPermissionSeederService
{
    Task SeedPermissionsAsync(CancellationToken cancellationToken = default);
}