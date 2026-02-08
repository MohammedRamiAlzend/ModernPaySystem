using System;
using System.Collections.Generic;
using System.Text;

namespace ModernPaySystem.Domain.Attrs;

public class TransactionPermissionAttribute(
    string key,
    string displayNameArabic,
    string displayNameEnglish,
    PermissionType type) : EndpointPermissionAttribute(
        key, SubSystem.TransactionSystem, type, displayNameArabic, displayNameEnglish)
{
    public SubSystem Subsystem { get; } = SubSystem.TransactionSystem;
}