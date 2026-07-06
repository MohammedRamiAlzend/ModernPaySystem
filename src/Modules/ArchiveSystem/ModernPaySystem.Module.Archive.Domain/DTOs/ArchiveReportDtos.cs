namespace ModernPaySystem.Module.Archive.Domain.DTOs;

public class DepartmentArchiveDashboardDto
{
    public int TotalFolders { get; set; }
    public int TotalRecords { get; set; }
    public int RecordsToday { get; set; }
    public int RecordsThisWeek { get; set; }
    public int RecordsThisMonth { get; set; }
    public int ActiveUsersToday { get; set; }
    public int ActiveUsersThisWeek { get; set; }
    public int ActiveUsersThisMonth { get; set; }
    public long TotalStorageBytes { get; set; }
    public Dictionary<string, int> StatusBreakdown { get; set; } = [];
}

public class ArchiveDailyReportDto
{
    public DateTime Date { get; set; }
    public int RecordsCreated { get; set; }
    public int FilesUploaded { get; set; }
    public int Views { get; set; }
    public int ActiveUsers { get; set; }
    public List<HourlyBreakdownDto> HourlyBreakdown { get; set; } = [];
}

public class ArchivePeriodReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalRecordsCreated { get; set; }
    public int TotalFilesUploaded { get; set; }
    public int TotalViews { get; set; }
    public int UniqueActiveUsers { get; set; }
    public List<DailyBreakdownItemDto> DailyBreakdown { get; set; } = [];
    public List<ArchiveUserSummaryDto> TopUsers { get; set; } = [];
}

public class ArchiveUserSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int RecordsCreated { get; set; }
    public int FilesUploaded { get; set; }
    public int TotalActions { get; set; }
}

public class UserActivityReportItemDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int RecordsCreated { get; set; }
    public int FilesUploaded { get; set; }
    public int TotalActions { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

public class ActiveUserReportItemDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int TotalActions { get; set; }
    public DateTime? FirstActionDate { get; set; }
    public DateTime? LastActionDate { get; set; }
    public List<string> ActionsPerformed { get; set; } = [];
}

public class StorageConsumptionReportDto
{
    public long TotalStorageBytes { get; set; }
    public int TotalFiles { get; set; }
    public List<StoragePerUserDto> PerUser { get; set; } = [];
    public List<StoragePerTypeDto> FileTypeBreakdown { get; set; } = [];
}

public class StoragePerUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public long TotalBytes { get; set; }
    public double PercentageOfTotal { get; set; }
    public DateTime? LastFileAddedAt { get; set; }
}

public class DepartmentChartsDataDto
{
    public List<ChartDataPointDto> DailyActivity { get; set; } = [];
    public List<ChartDataPointDto> ActionTypeBreakdown { get; set; } = [];
    public List<ChartDataPointDto> HourlyDistribution { get; set; } = [];
    public List<ChartDataPointDto> TopActiveUsers { get; set; } = [];
    public List<ChartDataPointDto> TopStorageUsers { get; set; } = [];
    public List<ChartDataPointDto> Trend7Days { get; set; } = [];
}

public class DailyWorkReportDto
{
    public DateTime Date { get; set; }
    public string? DepartmentName { get; set; }
    public List<DailyWorkAuditLogItemDto> AuditLogs { get; set; } = [];
    public List<DailyWorkArchiveRecordItemDto> Records { get; set; } = [];
}

public class DailyWorkAuditLogItemDto
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DailyWorkArchiveRecordItemDto
{
    public Guid Id { get; set; }
    public string? FolderName { get; set; }
    public string? UploaderName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DailyWorkFormValueItemDto> FormValues { get; set; } = [];
}

public class HourlyBreakdownDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class DailyBreakdownItemDto
{
    public DateTime Date { get; set; }
    public int Records { get; set; }
    public int FilesUploaded { get; set; }
}

public class ChartDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class DailyWorkFormValueItemDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}

public class StoragePerTypeDto
{
    public string FileType { get; set; } = string.Empty;
    public int Count { get; set; }
    public long TotalBytes { get; set; }
}
