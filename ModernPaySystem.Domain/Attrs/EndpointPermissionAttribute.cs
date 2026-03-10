namespace ModernPaySystem.Domain.Attrs;

[AttributeUsage(AttributeTargets.Method)]
public class EndpointPermissionAttribute(
    string key, SubSystem subSystem, PermissionType type, string? name = null, string? description = null) : Attribute
{
    public string? Name => name;
    public string? Description => description;
    public string Key => key;
    public SubSystem SubSystem => subSystem;
    public PermissionType Type => type;
}

public enum PermissionType
{
    Read,
    Insert,
    Delete,
    Update
}
