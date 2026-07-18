using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Application.Interfaces;

public interface IArchiveRecordReportService
{
    Task<Result<List<DepartmentDto>>> GetMyDepartmentsAsync();

    Task<Result<DepartmentArchiveDashboardDto>> GetDepartmentDashboardAsync();

    Task<Result<ArchiveDailyReportDto>> GetDailyReportAsync(DateTime? date);

    Task<Result<ArchivePeriodReportDto>> GetWeeklyReportAsync(DateTime? weekStart);

    Task<Result<ArchivePeriodReportDto>> GetMonthlyReportAsync(int? year, int? month);

    Task<Result<List<UserActivityReportItemDto>>> GetUserActivityReportAsync(
        DateTime? fromDate = null, DateTime? toDate = null);

    Task<Result<List<ActiveUserReportItemDto>>> GetActiveUsersAsync(
        DateTime? fromDate = null, DateTime? toDate = null);

    Task<Result<StorageConsumptionReportDto>> GetStorageConsumptionReportAsync();

    Task<Result<DepartmentChartsDataDto>> GetChartsDataAsync(
        DateTime? fromDate = null, DateTime? toDate = null);

    Task<Result<DailyWorkReportDto>> GetDailyWorkReportAsync(DateTime? date);
}
