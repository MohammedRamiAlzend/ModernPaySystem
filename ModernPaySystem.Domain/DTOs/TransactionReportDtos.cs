using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.DTOs;

public class TransactionDashboardDto
{
    public int TotalRequests { get; set; }
    public int Pending { get; set; }
    public int InProcess { get; set; }
    public int Managed { get; set; }
    public int Delivered { get; set; }
    public int TotalResponses { get; set; }
    public int TotalAttachments { get; set; }
    public int RequestsToday { get; set; }
    public int RequestsThisWeek { get; set; }
    public int RequestsThisMonth { get; set; }
    public int ResponsesToday { get; set; }
    public int ResponsesThisWeek { get; set; }
    public int ResponsesThisMonth { get; set; }
    public int ActiveUsersToday { get; set; }
    public int ActiveUsersThisWeek { get; set; }
    public int ActiveUsersThisMonth { get; set; }
    public Dictionary<string, int> StatusBreakdown { get; set; } = [];
}

public class TransactionDailyReportDto
{
    public DateTime Date { get; set; }
    public int RequestsCreated { get; set; }
    public int ResponsesMade { get; set; }
    public int AttachmentsAdded { get; set; }
    public int Views { get; set; }
    public int ActiveUsers { get; set; }
    public List<HourlyBreakdownDto> HourlyBreakdown { get; set; } = [];
}

public class TransactionPeriodReportDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int TotalRequestsCreated { get; set; }
    public int TotalResponsesMade { get; set; }
    public int TotalAttachmentsAdded { get; set; }
    public int TotalViews { get; set; }
    public int UniqueActiveUsers { get; set; }
    public List<DailyBreakdownItemDto> DailyBreakdown { get; set; } = [];
    public List<TransactionUserSummaryDto> TopUsers { get; set; } = [];
}

public class TransactionUserSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int RequestsCreated { get; set; }
    public int ResponsesMade { get; set; }
    public int TotalActions { get; set; }
}

public class TransactionUserActivityItemDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int RequestsCreated { get; set; }
    public int ResponsesMade { get; set; }
    public int AttachmentsAdded { get; set; }
    public int TotalActions { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

public class TransactionActiveUserItemDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int TotalActions { get; set; }
    public DateTime? FirstActionDate { get; set; }
    public DateTime? LastActionDate { get; set; }
    public List<string> ActionsPerformed { get; set; } = [];
}

public class TransactionStorageReportDto
{
    public long TotalStorageBytes { get; set; }
    public int TotalFiles { get; set; }
    public List<TransactionStoragePerUserDto> PerUser { get; set; } = [];
    public List<StoragePerTypeDto> FileTypeBreakdown { get; set; } = [];
}

public class TransactionStoragePerUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public long TotalBytes { get; set; }
    public double PercentageOfTotal { get; set; }
    public DateTime? LastFileAddedAt { get; set; }
}

public class TransactionChartsDataDto
{
    public List<ChartDataPointDto> DailyActivity { get; set; } = [];
    public List<ChartDataPointDto> ActionTypeBreakdown { get; set; } = [];
    public List<ChartDataPointDto> HourlyDistribution { get; set; } = [];
    public List<ChartDataPointDto> TopActiveUsers { get; set; } = [];
    public List<ChartDataPointDto> TopStorageUsers { get; set; } = [];
    public List<ChartDataPointDto> Trend7Days { get; set; } = [];
}

public class TransactionDailyWorkDto
{
    public DateTime Date { get; set; }
    public string? DepartmentName { get; set; }
    public List<TransactionDailyWorkAuditLogItemDto> AuditLogs { get; set; } = [];
    public List<TransactionDailyWorkRequestItemDto> Requests { get; set; } = [];
}

public class TransactionDailyWorkAuditLogItemDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public int? RequestNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

public class TransactionDailyWorkRequestItemDto
{
    public Guid Id { get; set; }
    public int RequestNumber { get; set; }
    public string? TemplateName { get; set; }
    public string? RequesterName { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DailyWorkFormValueItemDto> FormValues { get; set; } = [];
}
