using System;

namespace ModernPaySystem.Domain.Commons;

public static class Result
{
    public static Success Success => default;
    public static Created Created => default;
    public static Deleted Deleted => default;
    public static Updated Updated => default;
    public static Assigned Assigned => default;
}
