namespace ModernPaySystem.BuildingBlocks.Application.Abstractions;

public interface ICurrentUser
{
    string? UserId { get; }
}
