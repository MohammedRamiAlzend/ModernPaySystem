namespace ModernPaySystem.BuildingBlocks.Application.Abstractions;

public interface ITenantContext
{
    string? TenantId { get; }
}
