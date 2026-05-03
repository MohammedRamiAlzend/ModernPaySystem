namespace ModernPaySystem;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(policy: permission)
{
}
