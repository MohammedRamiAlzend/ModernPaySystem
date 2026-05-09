namespace ModernPaySystem.BuildingBlocks.Integration.Abstractions;

public interface IOutboxWriter
{
    Task WriteAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
