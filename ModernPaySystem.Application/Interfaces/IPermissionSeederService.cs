using System;
using System.Collections.Generic;
using System.Text;

namespace ModernPaySystem.Application.Interfaces;

public interface IPermissionSeederService
{
    Task SeedPermissionsAsync(CancellationToken cancellationToken = default);
}
