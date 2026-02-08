using System;
using System.Collections.Generic;
using System.Text;

namespace ModernPaySystem.Domain.Attrs;

public class TransactionPermissionAttribute(
    string key,
    string displayNameArabic,
    string displayNameEnglish,
    PermissionType type) : EndpointPermissionAttribute(
        key, displayNameArabic, displayNameEnglish, type)
{
    public SubSystem Subsystem { get; } = SubSystem.TransactionSystem;
}