namespace ModernPaySystem.BuildingBlocks.Integration.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
    string? TenantId { get; }
    string? CorrelationId { get; }
    string? CausationId { get; }
    int Version { get; }
}
