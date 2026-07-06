using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Domain.Attrs;

public class TransactionPermissionAttribute(
    string key,
    string displayNameArabic,
    string displayNameEnglish,
    PermissionType type) : EndpointPermissionAttribute(
        key, SubSystem.TransactionSystem, type, displayNameArabic, displayNameEnglish)
{
    public SubSystem Subsystem { get; } = SubSystem.TransactionSystem;
}
