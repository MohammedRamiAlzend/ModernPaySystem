using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.DTOs;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.DTOs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveRecordReportService(
    IArchiveUnitOfWork unitOfWork,
    ArchiveDbContext dbContext,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveAuthorizationService archiveAuthorizationService,
    ILogger<ArchiveRecordReportService> logger) : IArchiveRecordReportService
{
    public async Task<Result<List<DepartmentDto>>> GetMyDepartmentsAsync()
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var departments = departmentIds.Select(id => new DepartmentDto
            {
                Id = id,
                Name = id.ToString(),
                Code = null,
                Description = null,
                Level = 0,
                ChildrenCount = 0,
                UsersCount = 0,
                CreatedAt = null
            }).ToList();

            return departments;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting archive leader departments for current user");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DepartmentArchiveDashboardDto>> GetDepartmentDashboardAsync()
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var recordsQuery = dbContext.ArchiveRecords
                .AsNoTracking()
                .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value) && !r.IsDeleted);

            var totalRecords = await recordsQuery.CountAsync();
            var todayRecords = await recordsQuery.CountAsync(r => r.CreatedAt >= todayStart);
            var weekRecords = await recordsQuery.CountAsync(r => r.CreatedAt >= weekStart);
            var monthRecords = await recordsQuery.CountAsync(r => r.CreatedAt >= monthStart);

            var totalFolders = await dbContext.Folders
                .AsNoTracking()
                .CountAsync(f => f.DepartmentId != null && departmentIds.Contains(f.DepartmentId.Value) && !f.IsDeleted);

            var filesQuery = dbContext.PhysicalFiles
                .AsNoTracking()
                .Where(pf => pf.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(pf.ArchiveRecord.DepartmentId.Value) && !pf.IsDeleted);

            var totalFiles = await filesQuery.CountAsync();
            var totalStorageBytes = await filesQuery.SumAsync(pf => pf.FileSize);

            var auditQuery = dbContext.ArchiveAuditLogs
                .AsNoTracking()
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value));

            var todayActiveUsers = await auditQuery
                .Where(al => al.Timestamp >= todayStart)
                .Select(al => al.UserId).Distinct().CountAsync();

            var weekActiveUsers = await auditQuery
                .Where(al => al.Timestamp >= weekStart)
                .Select(al => al.UserId).Distinct().CountAsync();

            var monthActiveUsers = await auditQuery
                .Where(al => al.Timestamp >= monthStart)
                .Select(al => al.UserId).Distinct().CountAsync();

            var actionBreakdown = await auditQuery
                .GroupBy(al => al.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToListAsync();

            return new DepartmentArchiveDashboardDto
            {
                TotalFolders = totalFolders,
                TotalRecords = totalRecords,
                RecordsToday = todayRecords,
                RecordsThisWeek = weekRecords,
                RecordsThisMonth = monthRecords,
                ActiveUsersToday = todayActiveUsers,
                ActiveUsersThisWeek = weekActiveUsers,
                ActiveUsersThisMonth = monthActiveUsers,
                TotalStorageBytes = totalStorageBytes,
                StatusBreakdown = actionBreakdown.ToDictionary(x => x.Action.ToString(), x => x.Count)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting dashboard for current user");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveDailyReportDto>> GetDailyReportAsync(DateTime? date)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var dayStart = date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc).Date : DateTime.UtcNow.Date;
            var dayEnd = dayStart.AddDays(1);

            var recordsQuery = dbContext.ArchiveRecords
                .AsNoTracking()
                .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value));

            var recordsCreated = await recordsQuery
                .CountAsync(r => r.CreatedAt >= dayStart && r.CreatedAt < dayEnd);

            var auditQuery = dbContext.ArchiveAuditLogs
                .AsNoTracking()
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                    && al.Timestamp >= dayStart && al.Timestamp < dayEnd);

            var filesUploaded = await auditQuery
                .CountAsync(al => al.Action == AuditAction.AddFiles);

            var views = await auditQuery
                .CountAsync(al => al.Action == AuditAction.View);

            var activeUsers = await auditQuery
                .Select(al => al.UserId).Distinct().CountAsync();

            var hourlyData = await auditQuery
                .GroupBy(al => al.Timestamp.Hour)
                .Select(g => new HourlyBreakdownDto
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToListAsync();

            return new ArchiveDailyReportDto
            {
                Date = dayStart,
                RecordsCreated = recordsCreated,
                FilesUploaded = filesUploaded,
                Views = views,
                ActiveUsers = activeUsers,
                HourlyBreakdown = hourlyData
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting daily report");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchivePeriodReportDto>> GetWeeklyReportAsync(DateTime? weekStart)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var start = weekStart.HasValue ? DateTime.SpecifyKind(weekStart.Value, DateTimeKind.Utc).Date : StartOfWeek(DateTime.UtcNow);
            var end = start.AddDays(7);

            return await BuildPeriodReportAsync(departmentIds, start, end,
                $"Week of {start:MMM dd, yyyy}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting weekly report");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchivePeriodReportDto>> GetMonthlyReportAsync(int? year, int? month)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var now = DateTime.UtcNow;
            var reportYear = year ?? now.Year;
            var reportMonth = month ?? now.Month;

            if (reportYear < 2000 || reportYear > 2100 || reportMonth < 1 || reportMonth > 12)
                return ArchiveErrors.InvalidInput;

            var start = new DateTime(reportYear, reportMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            return await BuildPeriodReportAsync(departmentIds, start, end,
                $"{start:MMMM yyyy}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting monthly report");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<List<UserActivityReportItemDto>>> GetUserActivityReportAsync(
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var from = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc).Date : DateTime.UtcNow.Date.AddDays(-30);
            var to = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc).Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

            var auditQuery = dbContext.ArchiveAuditLogs
                .AsNoTracking()
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                    && al.Timestamp >= from && al.Timestamp < to);

            var userActions = await auditQuery
                .GroupBy(al => al.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    RecordsCreated = g.Count(al => al.Action == AuditAction.Create),
                    FilesUploaded = g.Count(al => al.Action == AuditAction.AddFiles),
                    TotalActions = g.Count(),
                    LastActivity = g.Max(al => al.Timestamp)
                })
                .ToListAsync();

            var result = userActions.Select(ua =>
            {
                var uid = Guid.TryParse(ua.UserId, out var id) ? id : Guid.Empty;
                return new UserActivityReportItemDto
                {
                    UserId = uid,
                    UserName = ua.UserId,
                    RecordsCreated = ua.RecordsCreated,
                    FilesUploaded = ua.FilesUploaded,
                    TotalActions = ua.TotalActions,
                    LastActivityDate = ua.LastActivity
                };
            })
            .OrderByDescending(u => u.TotalActions)
            .ToList();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user activity report");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<List<ActiveUserReportItemDto>>> GetActiveUsersAsync(
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var from = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc).Date : DateTime.UtcNow.Date.AddDays(-30);
            var to = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc).Date.AddDays(1) : DateTime.UtcNow.Date.AddDays(1);

            var auditQuery = dbContext.ArchiveAuditLogs
                .AsNoTracking()
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                    && al.Timestamp >= from && al.Timestamp < to);

            var userActions = await auditQuery
                .GroupBy(al => al.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalActions = g.Count(),
                    LastActionDate = g.Max(al => al.Timestamp),
                    FirstActionDate = g.Min(al => al.Timestamp),
                    ActionsPerformed = g.Select(al => al.Action.ToString()).Distinct().ToList()
                })
                .ToListAsync();

            var result = userActions.Select(ua =>
            {
                var uid = Guid.TryParse(ua.UserId, out var id) ? id : Guid.Empty;
                return new ActiveUserReportItemDto
                {
                    UserId = uid,
                    UserName = ua.UserId,
                    TotalActions = ua.TotalActions,
                    LastActionDate = ua.LastActionDate,
                    FirstActionDate = ua.FirstActionDate,
                    ActionsPerformed = ua.ActionsPerformed.ToList()
                };
            })
            .OrderByDescending(u => u.TotalActions)
            .ToList();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active users");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<StorageConsumptionReportDto>> GetStorageConsumptionReportAsync()
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;

            var filesQuery = dbContext.PhysicalFiles
                .AsNoTracking()
                .Where(pf => pf.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(pf.ArchiveRecord.DepartmentId.Value) && !pf.IsDeleted);

            var totalBytes = await filesQuery.SumAsync(pf => pf.FileSize);
            var totalFiles = await filesQuery.CountAsync();

            var fileData = await filesQuery
                .Select(pf => new { pf.CreatedByUserId, pf.FileSize, pf.FileExtension, pf.CreatedAt })
                .ToListAsync();

            var perUserData = fileData
                .GroupBy(pf => pf.CreatedByUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalFiles = g.Count(),
                    TotalBytes = g.Sum(pf => pf.FileSize),
                    LastFileAdded = g.Max(pf => pf.CreatedAt)
                })
                .ToList();

            var perUser = perUserData.Select(pu =>
            {
                var uid = Guid.TryParse(pu.UserId, out var id) ? id : Guid.Empty;
                return new StoragePerUserDto
                {
                    UserId = uid,
                    UserName = pu.UserId ?? "Unknown",
                    TotalFiles = pu.TotalFiles,
                    TotalBytes = pu.TotalBytes,
                    PercentageOfTotal = totalBytes > 0 ? Math.Round((double)pu.TotalBytes / totalBytes * 100, 2) : 0,
                    LastFileAddedAt = pu.LastFileAdded
                };
            })
            .OrderByDescending(pu => pu.TotalBytes)
            .ToList();

            var fileTypeData = await filesQuery
                .GroupBy(pf => pf.FileExtension)
                .Select(g => new
                {
                    Extension = g.Key,
                    Count = g.Count(),
                    TotalBytes = g.Sum(pf => pf.FileSize)
                })
                .OrderByDescending(g => g.TotalBytes)
                .ToListAsync();

            var fileTypeBreakdown = fileTypeData.Select(ft => new StoragePerTypeDto
            {
                FileType = ft.Extension ?? "unknown",
                Count = ft.Count,
                TotalBytes = ft.TotalBytes
            }).ToList();

            return new StorageConsumptionReportDto
            {
                TotalStorageBytes = totalBytes,
                TotalFiles = totalFiles,
                PerUser = perUser,
                FileTypeBreakdown = fileTypeBreakdown
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting storage report");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DepartmentChartsDataDto>> GetChartsDataAsync(
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var now = DateTime.UtcNow;
            var from = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc).Date : now.Date.AddDays(-30);
            var to = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc).Date.AddDays(1) : now.Date.AddDays(1);

            var auditQuery = dbContext.ArchiveAuditLogs
                .AsNoTracking()
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                    && al.Timestamp >= from && al.Timestamp < to);

            var auditRecords = await auditQuery
                .Select(al => new { al.Timestamp, al.Action, al.UserId })
                .ToListAsync();

            var dailyActivity = auditRecords
                .GroupBy(al => al.Timestamp.Date)
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Value = g.Count()
                })
                .OrderBy(d => d.Label)
                .ToList();

            var actionBreakdown = auditRecords
                .GroupBy(al => al.Action)
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key.ToString(),
                    Value = g.Count()
                })
                .ToList();

            var hourlyDistribution = auditRecords
                .GroupBy(al => al.Timestamp.Hour)
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key.ToString("D2") + ":00",
                    Value = g.Count()
                })
                .OrderBy(h => h.Label)
                .ToList();

            var topUsers = auditRecords
                .GroupBy(al => al.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(u => u.Count)
                .Take(10)
                .ToList();

            var topActiveUsers = topUsers.Select(tu =>
            {
                var uid = Guid.TryParse(tu.UserId, out var id) ? id : Guid.Empty;
                return new ChartDataPointDto
                {
                    Label = tu.UserId,
                    Value = tu.Count
                };
            }).ToList();

            var filesQuery = dbContext.PhysicalFiles
                .AsNoTracking()
                .Where(pf => pf.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(pf.ArchiveRecord.DepartmentId.Value) && !pf.IsDeleted);

            var topStorageUsers = await filesQuery
                .GroupBy(pf => pf.CreatedByUserId)
                .Select(g => new { UserId = g.Key, TotalBytes = g.Sum(pf => pf.FileSize) })
                .OrderByDescending(s => s.TotalBytes)
                .Take(10)
                .ToListAsync();

            var topStorageUsersChart = topStorageUsers.Select(tu =>
            {
                var uid = Guid.TryParse(tu.UserId, out var id) ? id : Guid.Empty;
                return new ChartDataPointDto
                {
                    Label = tu.UserId ?? "Unknown",
                    Value = tu.TotalBytes
                };
            }).ToList();

            var trendFrom = now.Date.AddDays(-6);
            var trendRecords = auditRecords
                .Where(al => al.Timestamp >= trendFrom)
                .ToList();

            var trend7Days = trendRecords
                .GroupBy(al => al.Timestamp.Date)
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Value = g.Count()
                })
                .OrderBy(d => d.Label)
                .ToList();

            return new DepartmentChartsDataDto
            {
                DailyActivity = dailyActivity,
                ActionTypeBreakdown = actionBreakdown,
                HourlyDistribution = hourlyDistribution,
                TopActiveUsers = topActiveUsers,
                TopStorageUsers = topStorageUsersChart,
                Trend7Days = trend7Days
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting charts data");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DailyWorkReportDto>> GetDailyWorkReportAsync(DateTime? date)
    {
        try
        {
            var deptIdsResult = await ResolveUserDepartmentIdsAsync();
            if (deptIdsResult.IsError)
                return deptIdsResult.Errors;

            var departmentIds = deptIdsResult.Value!;
            var dayStart = date.HasValue ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc).Date : DateTime.UtcNow.Date;
            var dayEnd = dayStart.AddDays(1);

            // Audit Logs
            var auditLogs = await dbContext.ArchiveAuditLogs
                .AsNoTracking()
                .Include(al => al.ArchiveRecord)
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                    && al.Timestamp >= dayStart && al.Timestamp < dayEnd)
                .OrderByDescending(al => al.Timestamp)
                .ToListAsync();

            var auditLogDtos = auditLogs.Select(al =>
            {
                var uid = Guid.TryParse(al.UserId, out var id) ? id : Guid.Empty;
                return new DailyWorkAuditLogItemDto
                {
                    Id = al.Id,
                    RecordId = al.ArchiveRecordId,
                    UserName = al.UserId,
                    Action = al.Action.ToString(),
                    Details = al.Details,
                    Timestamp = al.Timestamp
                };
            }).ToList();

            // Archive Records
            var records = await dbContext.ArchiveRecords
                .AsNoTracking()
                .Include(r => r.Folder)
                .Include(r => r.ArchiveRecordTemplateValuesId)
                    .ThenInclude(tv => tv!.ArchiveRecordFormInputValues)
                .Include(r => r.Form)
                .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value)
                    && r.CreatedAt >= dayStart && r.CreatedAt < dayEnd
                    && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var recordDtos = records.Select(r =>
            {
                var uid = Guid.TryParse(r.CreatedByUserId, out var id) ? id : Guid.Empty;

                return new DailyWorkArchiveRecordItemDto
                {
                    Id = r.Id,
                    FolderName = r.Folder?.Name,
                    UploaderName = r.CreatedByUserId,
                    CreatedAt = r.CreatedAt ?? dayStart,
                    UpdatedAt = r.UpdatedAt,
                    FormValues = r.ArchiveRecordTemplateValuesId?.ArchiveRecordFormInputValues
                        .Select(fv => new DailyWorkFormValueItemDto
                        {
                            Key = fv.Key,
                            Value = fv.Value
                        }).ToList() ?? []
                };
            }).ToList();

            return new DailyWorkReportDto
            {
                Date = dayStart,
                AuditLogs = auditLogDtos,
                Records = recordDtos
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting daily work report");
            return ArchiveErrors.InternalServerError;
        }
    }

    private async Task<Result<ArchivePeriodReportDto>> BuildPeriodReportAsync(
        List<Guid> departmentIds, DateTime periodStart, DateTime periodEnd, string periodLabel)
    {
        var recordsQuery = dbContext.ArchiveRecords
            .AsNoTracking()
            .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value));

        var totalCreated = await recordsQuery
            .CountAsync(r => r.CreatedAt >= periodStart && r.CreatedAt < periodEnd);

        var auditQuery = dbContext.ArchiveAuditLogs
            .AsNoTracking()
            .Where(al => al.ArchiveRecord.DepartmentId != null
                && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                && al.Timestamp >= periodStart && al.Timestamp < periodEnd);

        var filesUploaded = await auditQuery
            .CountAsync(al => al.Action == AuditAction.AddFiles);

        var views = await auditQuery
            .CountAsync(al => al.Action == AuditAction.View);

        var uniqueUsers = await auditQuery
            .Select(al => al.UserId).Distinct().CountAsync();

        var dailyData = await auditQuery
            .GroupBy(al => al.Timestamp.Date)
            .Select(g => new DailyBreakdownItemDto
            {
                Date = g.Key,
                Records = g.Count(al => al.Action == AuditAction.Create),
                FilesUploaded = g.Count(al => al.Action == AuditAction.AddFiles)
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var userActivity = await auditQuery
            .GroupBy(al => al.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                RecordsCreated = g.Count(al => al.Action == AuditAction.Create),
                FilesUploaded = g.Count(al => al.Action == AuditAction.AddFiles),
                TotalActions = g.Count(),
                LastActivity = g.Max(al => al.Timestamp)
            })
            .OrderByDescending(u => u.TotalActions)
            .Take(10)
            .ToListAsync();

        var topUsers = userActivity.Select(ua =>
        {
            var uid = Guid.TryParse(ua.UserId, out var id) ? id : Guid.Empty;
            return new ArchiveUserSummaryDto
            {
                UserId = uid,
                UserName = ua.UserId,
                RecordsCreated = ua.RecordsCreated,
                FilesUploaded = ua.FilesUploaded,
                TotalActions = ua.TotalActions
            };
        }).ToList();

        return new ArchivePeriodReportDto
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PeriodLabel = periodLabel,
            TotalRecordsCreated = totalCreated,
            TotalFilesUploaded = filesUploaded,
            TotalViews = views,
            UniqueActiveUsers = uniqueUsers,
            DailyBreakdown = dailyData,
            TopUsers = topUsers
        };
    }

    private async Task<Result<List<Guid>>> ResolveUserDepartmentIdsAsync()
    {
        var userId = httpContextServiceManager.GetCurrentUserId();
        if (userId == Guid.Empty)
            return ArchiveErrors.InvalidInput;

        var result = await archiveAuthorizationService.GetUserArchiveLeaderDepartmentsAsync(userId);
        if (result.IsError)
            return result.Errors;

        if (result.Value!.Count == 0)
            return ArchiveErrors.InternalServerError;

        return result.Value;
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}