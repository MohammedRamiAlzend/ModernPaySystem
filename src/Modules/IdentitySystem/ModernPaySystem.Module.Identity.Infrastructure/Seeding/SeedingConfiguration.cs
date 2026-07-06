namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public sealed class SeedingConfiguration
{
    public bool Enabled { get; set; } = true;
    public bool ClearExistingData { get; set; }
    public string Environment { get; set; } = "Development";
}