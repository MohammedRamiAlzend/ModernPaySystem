using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Domain.Attrs;

public class ArchivePermissionAttribute(
    string key,
    string displayNameArabic,
    string displayNameEnglish,
    PermissionType type) : EndpointPermissionAttribute(
        key, SubSystem.Archiving, type, displayNameArabic, displayNameEnglish)
{
    public SubSystem Subsystem { get; } = SubSystem.Archiving;
}
