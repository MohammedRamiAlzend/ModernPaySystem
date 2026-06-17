using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveRecordReportService(
    IUnitOfWork unitOfWork,
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

            var departments = await unitOfWork.Context.Departments
                .AsNoTracking()
                .Where(d => deptIdsResult.Value!.Contains(d.Id))
                .Select(d => d.MapToDto())
                .ToListAsync();

            return departments;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting archive leader departments for current user");
            return ApplicationErrors.InternalServerError;
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

            var firstDept = await unitOfWork.Context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == departmentIds[0]);

            var recordsQuery = unitOfWork.Context.ArchiveRecords
                .AsNoTracking()
                .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value) && !r.IsDeleted);

            var totalRecords = await recordsQuery.CountAsync();
            var todayRecords = await recordsQuery.CountAsync(r => r.CreatedAt >= todayStart);
            var weekRecords = await recordsQuery.CountAsync(r => r.CreatedAt >= weekStart);
            var monthRecords = await recordsQuery.CountAsync(r => r.CreatedAt >= monthStart);

            var totalUsers = await unitOfWork.Context.Users
                .AsNoTracking()
                .CountAsync(u => u.DepartmentId != null && departmentIds.Contains(u.DepartmentId.Value));

            var totalFolders = await unitOfWork.Context.Folders
                .AsNoTracking()
                .CountAsync(f => f.DepartmentId != null && departmentIds.Contains(f.DepartmentId.Value) && !f.IsDeleted);

            var filesQuery = unitOfWork.Context.PhysicalFiles
                .AsNoTracking()
                .Where(pf => pf.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(pf.ArchiveRecord.DepartmentId.Value) && !pf.IsDeleted);

            var totalFiles = await filesQuery.CountAsync();
            var totalStorageBytes = await filesQuery.SumAsync(pf => pf.FileSize);

            var auditQuery = unitOfWork.Context.ArchiveAuditLogs
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
                DepartmentId = departmentIds[0],
                DepartmentName = firstDept?.Name ?? "Unknown",
                TotalArchiveRecords = totalRecords,
                TotalUsers = totalUsers,
                TotalFolders = totalFolders,
                TotalPhysicalFiles = totalFiles,
                TotalStorageBytes = totalStorageBytes,
                RecordsCreatedToday = todayRecords,
                RecordsCreatedThisWeek = weekRecords,
                RecordsCreatedThisMonth = monthRecords,
                ActiveUsersToday = todayActiveUsers,
                ActiveUsersThisWeek = weekActiveUsers,
                ActiveUsersThisMonth = monthActiveUsers,
                ActionTypeBreakdown = actionBreakdown.ToDictionary(x => x.Action.ToString(), x => x.Count)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting dashboard for current user");
            return ApplicationErrors.InternalServerError;
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

            var recordsQuery = unitOfWork.Context.ArchiveRecords
                .AsNoTracking()
                .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value));

            var recordsCreated = await recordsQuery
                .CountAsync(r => r.CreatedAt >= dayStart && r.CreatedAt < dayEnd);

            var recordsDeleted = await recordsQuery
                .CountAsync(r => r.DeletedAt >= dayStart && r.DeletedAt < dayEnd);

            var auditQuery = unitOfWork.Context.ArchiveAuditLogs
                .AsNoTracking()
                .Where(al => al.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                    && al.Timestamp >= dayStart && al.Timestamp < dayEnd);

            var filesAdded = await auditQuery
                .CountAsync(al => al.Action == AuditAction.AddFiles);

            var filesDownloaded = await auditQuery
                .CountAsync(al => al.Action == AuditAction.Download);

            var printActions = await auditQuery
                .CountAsync(al => al.Action == AuditAction.Print);

            var views = await auditQuery
                .CountAsync(al => al.Action == AuditAction.View);

            var activeUsers = await auditQuery
                .Select(al => al.UserId).Distinct().CountAsync();

            var hourlyData = await auditQuery
                .GroupBy(al => al.Timestamp.Hour)
                .Select(g => new HourlyBreakdownDto
                {
                    Hour = g.Key,
                    Actions = g.Count(),
                    RecordsCreated = g.Count(al => al.Action == AuditAction.Create)
                })
                .OrderBy(h => h.Hour)
                .ToListAsync();

            return new ArchiveDailyReportDto
            {
                Date = dayStart,
                RecordsCreated = recordsCreated,
                RecordsDeleted = recordsDeleted,
                FilesAdded = filesAdded,
                FilesDownloaded = filesDownloaded,
                PrintActions = printActions,
                Views = views,
                ActiveUsers = activeUsers,
                HourlyBreakdown = hourlyData
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting daily report");
            return ApplicationErrors.InternalServerError;
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
            return ApplicationErrors.InternalServerError;
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
                return ApplicationErrors.InvalidInput;

            var start = new DateTime(reportYear, reportMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            return await BuildPeriodReportAsync(departmentIds, start, end,
                $"{start:MMMM yyyy}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting monthly report");
            return ApplicationErrors.InternalServerError;
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

            var auditQuery = unitOfWork.Context.ArchiveAuditLogs
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
                    RecordsViewed = g.Count(al => al.Action == AuditAction.View),
                    FilesDownloaded = g.Count(al => al.Action == AuditAction.Download),
                    PrintActions = g.Count(al => al.Action == AuditAction.Print),
                    TotalActions = g.Count(),
                    LastActivity = g.Max(al => al.Timestamp)
                })
                .ToListAsync();

            var userIds = userActions
                .Select(u => Guid.TryParse(u.UserId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var users = await unitOfWork.Context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            var userMap = users.ToDictionary(u => u.Id, u => u.UserName);

            var result = userActions.Select(ua =>
            {
                var uid = Guid.TryParse(ua.UserId, out var id) ? id : Guid.Empty;
                return new UserActivityReportItemDto
                {
                    UserId = uid,
                    UserName = uid != Guid.Empty && userMap.TryGetValue(uid, out var name) ? name : ua.UserId,
                    RecordsCreated = ua.RecordsCreated,
                    RecordsViewed = ua.RecordsViewed,
                    FilesDownloaded = ua.FilesDownloaded,
                    PrintActions = ua.PrintActions,
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
            return ApplicationErrors.InternalServerError;
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

            var auditQuery = unitOfWork.Context.ArchiveAuditLogs
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
                    ActionsPerformed = g.Select(al => al.Action).Distinct().ToList()
                })
                .ToListAsync();

            var userIds = userActions
                .Select(u => Guid.TryParse(u.UserId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var users = await unitOfWork.Context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, DepartmentName = u.Department!.Name })
                .ToListAsync();

            var userMap = users.ToDictionary(u => u.Id, u => (u.UserName, u.DepartmentName));

            var result = userActions.Select(ua =>
            {
                var uid = Guid.TryParse(ua.UserId, out var id) ? id : Guid.Empty;
                var (userName, deptName) = uid != Guid.Empty && userMap.TryGetValue(uid, out var info)
                    ? info : (ua.UserId, (string?)null);
                return new ActiveUserReportItemDto
                {
                    UserId = uid,
                    UserName = userName,
                    DepartmentName = deptName,
                    TotalActions = ua.TotalActions,
                    LastActionDate = ua.LastActionDate,
                    FirstActionDate = ua.FirstActionDate,
                    ActionsPerformed = ua.ActionsPerformed.Select(a => a.ToString()).ToList()
                };
            })
            .OrderByDescending(u => u.TotalActions)
            .ToList();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active users");
            return ApplicationErrors.InternalServerError;
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

            var filesQuery = unitOfWork.Context.PhysicalFiles
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
                    FileTypeCounts = g.GroupBy(pf => pf.FileExtension ?? "unknown")
                        .ToDictionary(fg => fg.Key, fg => fg.Count()),
                    LastFileAdded = g.Max(pf => pf.CreatedAt)
                })
                .ToList();

            var userIds = perUserData
                .Select(u => Guid.TryParse(u.UserId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var users = await unitOfWork.Context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            var userMap = users.ToDictionary(u => u.Id, u => u.UserName);

            var perUser = perUserData.Select(pu =>
            {
                var uid = Guid.TryParse(pu.UserId, out var id) ? id : Guid.Empty;
                return new StoragePerUserDto
                {
                    UserId = uid,
                    UserName = uid != Guid.Empty && userMap.TryGetValue(uid, out var name) ? name : pu.UserId ?? "Unknown",
                    TotalFiles = pu.TotalFiles,
                    TotalBytes = pu.TotalBytes,
                    PercentageOfTotal = totalBytes > 0 ? Math.Round((double)pu.TotalBytes / totalBytes * 100, 2) : 0,
                    FileTypeCounts = pu.FileTypeCounts,
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
                Extension = ft.Extension ?? "unknown",
                Count = ft.Count,
                TotalBytes = ft.TotalBytes,
                PercentageOfTotal = totalBytes > 0 ? Math.Round((double)ft.TotalBytes / totalBytes * 100, 2) : 0
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
            return ApplicationErrors.InternalServerError;
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

            var auditQuery = unitOfWork.Context.ArchiveAuditLogs
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

            var topUsers = await auditQuery
                .GroupBy(al => al.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(u => u.Count)
                .Take(10)
                .ToListAsync();

            var userIds = topUsers
                .Select(u => Guid.TryParse(u.UserId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var users = await unitOfWork.Context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            var userMap = users.ToDictionary(u => u.Id, u => u.UserName);

            var topActiveUsers = topUsers.Select(tu =>
            {
                var uid = Guid.TryParse(tu.UserId, out var id) ? id : Guid.Empty;
                return new ChartDataPointDto
                {
                    Label = uid != Guid.Empty && userMap.TryGetValue(uid, out var name) ? name : tu.UserId,
                    Value = tu.Count
                };
            }).ToList();

            var filesQuery = unitOfWork.Context.PhysicalFiles
                .AsNoTracking()
                .Where(pf => pf.ArchiveRecord.DepartmentId != null
                    && departmentIds.Contains(pf.ArchiveRecord.DepartmentId.Value) && !pf.IsDeleted);

            var topStorageUsers = await filesQuery
                .GroupBy(pf => pf.CreatedByUserId)
                .Select(g => new { UserId = g.Key, TotalBytes = g.Sum(pf => pf.FileSize) })
                .OrderByDescending(s => s.TotalBytes)
                .Take(10)
                .ToListAsync();

            var storageUserIds = topStorageUsers
                .Select(u => Guid.TryParse(u.UserId, out var id) ? id : Guid.Empty)
                .Where(id => id != Guid.Empty)
                .ToList();

            var storageUsers = await unitOfWork.Context.Users
                .AsNoTracking()
                .Where(u => storageUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            var storageUserMap = storageUsers.ToDictionary(u => u.Id, u => u.UserName);

            var topStorageUsersChart = topStorageUsers.Select(tu =>
            {
                var uid = Guid.TryParse(tu.UserId, out var id) ? id : Guid.Empty;
                return new ChartDataPointDto
                {
                    Label = uid != Guid.Empty && storageUserMap.TryGetValue(uid, out var name) ? name : tu.UserId ?? "Unknown",
                    Value = tu.TotalBytes
                };
            }).ToList();

            var trendFrom = now.Date.AddDays(-6);
            var trendRecords = await auditQuery
                .Where(al => al.Timestamp >= trendFrom)
                .Select(al => new { al.Timestamp })
                .ToListAsync();

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
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task<Result<ArchivePeriodReportDto>> BuildPeriodReportAsync(
        List<Guid> departmentIds, DateTime periodStart, DateTime periodEnd, string periodLabel)
    {
        var recordsQuery = unitOfWork.Context.ArchiveRecords
            .AsNoTracking()
            .Where(r => r.DepartmentId != null && departmentIds.Contains(r.DepartmentId.Value));

        var totalCreated = await recordsQuery
            .CountAsync(r => r.CreatedAt >= periodStart && r.CreatedAt < periodEnd);

        var totalDeleted = await recordsQuery
            .CountAsync(r => r.DeletedAt >= periodStart && r.DeletedAt < periodEnd);

        var auditQuery = unitOfWork.Context.ArchiveAuditLogs
            .AsNoTracking()
            .Where(al => al.ArchiveRecord.DepartmentId != null
                && departmentIds.Contains(al.ArchiveRecord.DepartmentId.Value)
                && al.Timestamp >= periodStart && al.Timestamp < periodEnd);

        var filesAdded = await auditQuery
            .CountAsync(al => al.Action == AuditAction.AddFiles);

        var downloads = await auditQuery
            .CountAsync(al => al.Action == AuditAction.Download);

        var prints = await auditQuery
            .CountAsync(al => al.Action == AuditAction.Print);

        var views = await auditQuery
            .CountAsync(al => al.Action == AuditAction.View);

        var uniqueUsers = await auditQuery
            .Select(al => al.UserId).Distinct().CountAsync();

        var dailyData = await auditQuery
            .GroupBy(al => al.Timestamp.Date)
            .Select(g => new DailyBreakdownItemDto
            {
                Date = g.Key,
                Actions = g.Count(),
                RecordsCreated = g.Count(al => al.Action == AuditAction.Create),
                ActiveUsers = g.Select(al => al.UserId).Distinct().Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        var userActivity = await auditQuery
            .GroupBy(al => al.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                RecordsCreated = g.Count(al => al.Action == AuditAction.Create),
                RecordsViewed = g.Count(al => al.Action == AuditAction.View),
                FilesDownloaded = g.Count(al => al.Action == AuditAction.Download),
                PrintActions = g.Count(al => al.Action == AuditAction.Print),
                TotalActions = g.Count(),
                LastActivity = g.Max(al => al.Timestamp)
            })
            .OrderByDescending(u => u.TotalActions)
            .Take(10)
            .ToListAsync();

        var userIds = userActivity
            .Select(u => Guid.TryParse(u.UserId, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var users = await unitOfWork.Context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        var userMap = users.ToDictionary(u => u.Id, u => u.UserName);

        var topUsers = userActivity.Select(ua =>
        {
            var uid = Guid.TryParse(ua.UserId, out var id) ? id : Guid.Empty;
            return new UserActivitySummaryDto
            {
                UserId = uid,
                UserName = uid != Guid.Empty && userMap.TryGetValue(uid, out var name) ? name : ua.UserId,
                RecordsCreated = ua.RecordsCreated,
                RecordsViewed = ua.RecordsViewed,
                FilesDownloaded = ua.FilesDownloaded,
                PrintActions = ua.PrintActions,
                TotalActions = ua.TotalActions,
                LastActivityDate = ua.LastActivity
            };
        }).ToList();

        return new ArchivePeriodReportDto
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PeriodLabel = periodLabel,
            TotalRecordsCreated = totalCreated,
            TotalRecordsDeleted = totalDeleted,
            TotalFilesAdded = filesAdded,
            TotalDownloads = downloads,
            TotalPrints = prints,
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
            return ApplicationErrors.InvalidInput;

        var result = await archiveAuthorizationService.GetUserArchiveLeaderDepartmentsAsync(userId);
        if (result.IsError)
            return result.Errors;

        if (result.Value!.Count == 0)
            return ApplicationErrors.InsufficientPermissions;

        return result.Value;
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
