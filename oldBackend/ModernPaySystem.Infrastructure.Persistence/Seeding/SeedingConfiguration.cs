namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Configuration for database seeding
/// </summary>
public class SeedingConfiguration
{
    /// <summary>
    /// Environment mode: Development or Production
    /// </summary>
    public SeedingEnvironment Environment { get; set; } = SeedingEnvironment.Development;

    /// <summary>
    /// Whether to seed the database
    /// /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Number of entities to seed
    /// /// </summary>
    public SeedingQuantities Quantities { get; set; } = new();

    /// <summary>
    /// Whether to clear existing data before seeding
    /// /// </summary>
    public bool ClearExistingData { get; set; } = false;
}

/// <summary>
/// Seeding environment modes
/// /// </summary>
public enum SeedingEnvironment
{
    /// <summary>
    /// Development environment - more test data
    /// /// </summary>
    Development = 0,

    /// <summary>
    /// Production environment - minimal required data
    /// /// </summary>
    Production = 1
}

/// <summary>
/// Quantities of entities to seed
/// /// </summary>
public class SeedingQuantities
{
    /// <summary>
    /// Number of roles to create (default: 5)
    /// /// </summary>
    public int RoleCount { get; set; } = 5;

    /// <summary>
    /// Number of permissions to create (default: 20)
    /// /// </summary>
    public int PermissionCount { get; set; } = 20;

    /// <summary>
    /// Number of users to create (default: 25)
    /// /// </summary>
    public int UserCount { get; set; } = 25;

    /// <summary>
    /// Number of subsystems to create (default: 3)
    /// /// </summary>
    public int SubSystemCount { get; set; } = 3;

    /// <summary>
    /// Number of templates to create (default: 10)
    /// /// </summary>
    public int TemplateCount { get; set; } = 10;

    /// <summary>
    /// Number of requests to create (default: 50)
    /// /// </summary>
    public int RequestCount { get; set; } = 50;

    /// <summary>
    /// Number of responses to create (default: 30)
    /// /// </summary>
    public int ResponseCount { get; set; } = 30;

    /// <summary>
    /// Number of attachments to create (default: 20)
    /// /// </summary>
    public int AttachmentCount { get; set; } = 20;

    /// <summary>
    /// Adjust quantities based on environment
    /// /// </summary>
    public void ApplyEnvironmentDefaults(SeedingEnvironment environment)
    {
        if (environment == SeedingEnvironment.Production)
        {
            // Production uses minimal data
            RoleCount = 3;
            PermissionCount = 10;
            UserCount = 10;
            SubSystemCount = 1;
            TemplateCount = 2;
            RequestCount = 5;
            ResponseCount = 3;
            AttachmentCount = 2;
        }
    }
}
