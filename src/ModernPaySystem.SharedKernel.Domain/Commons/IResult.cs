namespace ModernPaySystem.SharedKernel.Domain.Commons;

public interface IResult
{
    List<Error>? Errors { get; }
    bool IsSuccess { get; }
}
