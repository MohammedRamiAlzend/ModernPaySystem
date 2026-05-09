namespace ModernPaySystem.BuildingBlocks.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
