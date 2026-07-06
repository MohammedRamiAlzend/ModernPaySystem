using ModernPaySystem.Module.Transaction.Domain.DTOs;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface IReportService
{
    Task<Result<PagedList<RequestDto>>> GetRequestsReportPaged(
        int pageNumber,
        int pageSize,
        DateTime? startDate,
        DateTime? endDate,
        bool forCurrentDepartment = false);

    Task<Result<PagedList<ResponseDto>>> GetResponsesReportPaged(
        int pageNumber,
        int pageSize,
        DateTime? startDate,
        DateTime? endDate,
        bool forCurrentDepartment = false);

    Task<Result<TransactionDashboardDto>> GetDashboardAsync();

    Task<Result<TransactionDailyReportDto>> GetDailyReportAsync(DateTime? date);

    Task<Result<TransactionPeriodReportDto>> GetWeeklyReportAsync(DateTime? weekStart);

    Task<Result<TransactionPeriodReportDto>> GetMonthlyReportAsync(int? year, int? month);

    Task<Result<List<TransactionUserActivityItemDto>>> GetUserActivityReportAsync(DateTime? fromDate, DateTime? toDate);

    Task<Result<List<TransactionActiveUserItemDto>>> GetActiveUsersAsync(DateTime? fromDate, DateTime? toDate);

    Task<Result<TransactionStorageReportDto>> GetStorageReportAsync();

    Task<Result<TransactionChartsDataDto>> GetChartsDataAsync(DateTime? fromDate, DateTime? toDate);

    Task<Result<TransactionDailyWorkDto>> GetDailyWorkReportAsync(DateTime? date);
}
