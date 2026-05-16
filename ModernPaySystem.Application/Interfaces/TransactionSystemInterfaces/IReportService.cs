global using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Domain.Commons;

namespace ModernPaySystem.Application.Interfaces.TransactionSystemInterfaces;

public interface IReportService
{
    Task<Result<PagedList<RequestDto>>> GetRequestsReportPaged(
        int pageNumber,
        int pageSize,
        DateTime? startDate,
        DateTime? endDate);

    Task<Result<PagedList<ResponseDto>>> GetResponsesReportPaged(
        int pageNumber,
        int pageSize,
        DateTime? startDate,
        DateTime? endDate);
}
