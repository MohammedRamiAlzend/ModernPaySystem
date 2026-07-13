using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.Module.Transaction.Domain.DTOs;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;
using System.Linq.Expressions;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class ReportService(
    ITransactionUnitOfWork unitOfWork,
    ILogger<ReportService> logger,
    IHttpContextServiceManager httpContextServiceManager) : IReportService
{
    public async Task<Result<PagedList<RequestDto>>> GetRequestsReportPaged(
        int pageNumber,
        int pageSize,
        DateTime? startDate,
        DateTime? endDate,
        bool forCurrentDepartment)
    {
        try
        {
            logger.LogInformation("Fetching requests report, page: {Page}, size: {PageSize}, startDate: {StartDate}, endDate: {EndDate}, forCurrentDepartment: {ForCurrentDepartment}",
                pageNumber, pageSize, startDate, endDate, forCurrentDepartment);

            if (pageNumber <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            if (startDate.HasValue)
                startDate = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            if (endDate.HasValue)
                endDate = DateTime.SpecifyKind(endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Utc);

            if (startDate.HasValue && !endDate.HasValue)
                endDate = DateTime.UtcNow;

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                return TransactionErrors.StartDateMustBeEarlier;

            List<Expression<Func<Request, bool>>> filters = [];

            if (startDate.HasValue)
                filters.Add(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                filters.Add(r => r.CreatedAt <= endDate.Value);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            if (forCurrentDepartment)
            {
                filters.Add(r => r.RequesterDepartmentId == currentUserId);
            }
            else
            {
                filters.Add(r => r.RequesterId == currentUserId);
            }

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                pageNumber,
                pageSize,
                transform: i => i.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: filters);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests report, page: {Page}, size: {PageSize}", pageNumber, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ResponseDto>>> GetResponsesReportPaged(
        int pageNumber,
        int pageSize,
        DateTime? startDate,
        DateTime? endDate,
        bool forCurrentDepartment)
    {
        try
        {
            logger.LogInformation("Fetching responses report, page: {Page}, size: {PageSize}, startDate: {StartDate}, endDate: {EndDate}, forCurrentDepartment: {ForCurrentDepartment}",
                pageNumber, pageSize, startDate, endDate, forCurrentDepartment);

            if (pageNumber <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            if (startDate.HasValue)
                startDate = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            if (endDate.HasValue)
                endDate = DateTime.SpecifyKind(endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Utc);

            if (startDate.HasValue && !endDate.HasValue)
                endDate = DateTime.UtcNow;

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                return TransactionErrors.StartDateMustBeEarlier;

            List<Expression<Func<Response, bool>>> filters = [];

            if (startDate.HasValue)
                filters.Add(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                filters.Add(r => r.CreatedAt <= endDate.Value);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            if (forCurrentDepartment)
            {
                filters.Add(r => r.Request != null && r.Request.RequesterDepartmentId == currentUserId);
            }
            else
            {
                filters.Add(r => r.Request != null && r.Request.RequesterId == currentUserId);
            }

            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(
                pageNumber,
                pageSize,
                transform: i => i
                    .Include(x => x.Request).ThenInclude(r => r!.RequestAttachments)
                    .Include(x => x.Request).ThenInclude(r => r!.RequestTemplateValues).ThenInclude(x => x!.Template)
                    .Include(x => x.Request).ThenInclude(r => r!.RequestAttachments)
                    .Include(x => x.Request).ThenInclude(r => r!.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: filters);

            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            var responseDtos = pagedResponses.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<ResponseDto>(responseDtos, pagedResponses.Value.TotalItems, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching responses report, page: {Page}, size: {PageSize}", pageNumber, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionDashboardDto>> GetDashboardAsync()
    {
        try
        {
            var departmentId = httpContextServiceManager.GetCurrentUserId();

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var requests = await unitOfWork.Requests.GetAllAsync(
                r => r.RequesterId == departmentId,
                transform: q => q.Include(r => r.RequestAttachments).AsNoTracking());

            if (requests.IsError)
                return requests.Errors;

            var allRequests = requests.Value!;
            var responses = await unitOfWork.Responses.GetAllAsync(
                r => r.Request != null && r.Request.RequesterId == departmentId,
                transform: q => q.AsNoTracking());

            var dashboard = new TransactionDashboardDto
            {
                TotalRequests = allRequests.Count,
                Pending = allRequests.Count(r => r.Status == RequestStatus.Pending),
                InProcess = allRequests.Count(r => r.Status == RequestStatus.InProcess),
                Managed = allRequests.Count(r => r.Status == RequestStatus.Managed),
                Delivered = allRequests.Count(r => r.Status == RequestStatus.Delivered),
                TotalResponses = responses.IsError ? 0 : responses.Value!.Count,
                TotalAttachments = allRequests.Sum(r => r.RequestAttachments?.Count ?? 0),
                RequestsToday = allRequests.Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value >= todayStart),
                RequestsThisWeek = allRequests.Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value >= weekStart),
                RequestsThisMonth = allRequests.Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value >= monthStart),
                StatusBreakdown = allRequests.GroupBy(r => r.Status.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            if (!responses.IsError && responses.Value != null)
            {
                dashboard.ResponsesToday = responses.Value.Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value >= todayStart);
                dashboard.ResponsesThisWeek = responses.Value.Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value >= weekStart);
                dashboard.ResponsesThisMonth = responses.Value.Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value >= monthStart);
            }

            var todayLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
                al => al.Timestamp >= todayStart,
                transform: q => q.Include(al => al.Request!).AsNoTracking());

            if (!todayLogs.IsError && todayLogs.Value != null)
            {
                var userLogs = todayLogs.Value
                    .Where(al => al.Request != null && al.Request.RequesterId == departmentId)
                    .ToList();
                dashboard.ActiveUsersToday = userLogs.Select(al => al.UserId).Distinct().Count();

                var weekLogs = userLogs.Where(al => al.Timestamp >= weekStart).ToList();
                dashboard.ActiveUsersThisWeek = weekLogs.Select(al => al.UserId).Distinct().Count();

                var monthLogs = userLogs.Where(al => al.Timestamp >= monthStart).ToList();
                dashboard.ActiveUsersThisMonth = monthLogs.Select(al => al.UserId).Distinct().Count();
            }

            return dashboard;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching dashboard");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionDailyReportDto>> GetDailyReportAsync(DateTime? date)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var reportDate = (date ?? DateTime.UtcNow).Date;
            var dayEnd = reportDate.AddDays(1);

            var requests = await unitOfWork.Requests.GetAllAsync(
                r => r.RequesterId == currentUserId
                    && r.CreatedAt.HasValue
                    && r.CreatedAt.Value >= reportDate
                    && r.CreatedAt.Value < dayEnd,
                transform: q => q.AsNoTracking());

            var responses = await unitOfWork.Responses.GetAllAsync(
                r => r.Request != null
                    && r.Request.RequesterId == currentUserId
                    && r.CreatedAt.HasValue
                    && r.CreatedAt.Value >= reportDate
                    && r.CreatedAt.Value < dayEnd,
                transform: q => q.AsNoTracking());

            var auditLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
                al => al.Timestamp >= reportDate && al.Timestamp < dayEnd,
                transform: q => q.Include(al => al.Request!).AsNoTracking());

            var userAuditLogs = auditLogs.IsError || auditLogs.Value is null
                ? []
                : auditLogs.Value.Where(al => al.Request != null && al.Request.RequesterId == currentUserId).ToList();

            var hourlyBreakdown = Enumerable.Range(0, 24).Select(hour => new HourlyBreakdownDto
            {
                Hour = hour,
                Count = (requests.IsError ? [] : requests.Value!)
                    .Count(r => r.CreatedAt.HasValue && r.CreatedAt.Value.Hour == hour)
            }).ToList();

            return new TransactionDailyReportDto
            {
                Date = reportDate,
                RequestsCreated = requests.IsError ? 0 : requests.Value!.Count,
                ResponsesMade = responses.IsError ? 0 : responses.Value!.Count,
                Views = userAuditLogs.Count(al => al.Action == RequestAuditAction.Viewed),
                ActiveUsers = userAuditLogs.Select(al => al.UserId).Distinct().Count(),
                HourlyBreakdown = hourlyBreakdown,
                AttachmentsAdded = userAuditLogs.Count(al => al.Action == RequestAuditAction.AttachmentAdded)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching daily report");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionPeriodReportDto>> GetWeeklyReportAsync(DateTime? weekStart)
    {
        try
        {
            var start = (weekStart ?? StartOfWeek(DateTime.UtcNow)).Date;
            var end = start.AddDays(7);
            return await BuildPeriodReportAsync(start, end, "Weekly");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching weekly report");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionPeriodReportDto>> GetMonthlyReportAsync(int? year, int? month)
    {
        try
        {
            var now = DateTime.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;
            var start = new DateTime(targetYear, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var monthName = new System.Globalization.CultureInfo("ar-SA").DateTimeFormat.GetMonthName(targetMonth);
            return await BuildPeriodReportAsync(start, end, $"{monthName} {targetYear}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching monthly report");
            return TransactionErrors.InternalServerError;
        }
    }

    private async Task<Result<TransactionPeriodReportDto>> BuildPeriodReportAsync(DateTime periodStart, DateTime periodEnd, string label)
    {
        var currentUserId = httpContextServiceManager.GetCurrentUserId();

        var requests = await unitOfWork.Requests.GetAllAsync(
            r => r.RequesterId == currentUserId
                && r.CreatedAt.HasValue
                && r.CreatedAt.Value >= periodStart
                && r.CreatedAt.Value < periodEnd,
            transform: q => q.AsNoTracking());

        var responses = await unitOfWork.Responses.GetAllAsync(
            r => r.Request != null
                && r.Request.RequesterId == currentUserId
                && r.CreatedAt.HasValue
                && r.CreatedAt.Value >= periodStart
                && r.CreatedAt.Value < periodEnd,
            transform: q => q.AsNoTracking());

        var auditLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
            al => al.Timestamp >= periodStart && al.Timestamp < periodEnd,
            transform: q => q.Include(al => al.Request!).AsNoTracking());

        var userAuditLogs = auditLogs.IsError || auditLogs.Value is null
            ? []
            : auditLogs.Value.Where(al => al.Request != null && al.Request.RequesterId == currentUserId).ToList();

        var dailyBreakdown = new List<DailyBreakdownItemDto>();
        for (var day = periodStart; day < periodEnd; day = day.AddDays(1))
        {
            var dayEnd = day.AddDays(1);
            var dayRequests = requests.IsError ? 0 : requests.Value!.Count(r => r.CreatedAt!.Value >= day && r.CreatedAt.Value < dayEnd);
            var dayLogs = userAuditLogs.Where(al => al.Timestamp >= day && al.Timestamp < dayEnd).ToList();

            dailyBreakdown.Add(new DailyBreakdownItemDto
            {
                Date = day,
                Requests = dayRequests,
                Responses = dayLogs.Count
            });
        }

        var userActionCounts = userAuditLogs
            .GroupBy(al => al.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        var topUsers = userActionCounts.Select(x => new TransactionUserSummaryDto
        {
            UserId = x.UserId,
            UserName = x.UserId.ToString(),
            TotalActions = x.Count
        }).ToList();

        var allRequests = requests.IsError ? [] : requests.Value!;
        var allResponses = responses.IsError ? [] : responses.Value!;

        return new TransactionPeriodReportDto
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PeriodLabel = label,
            TotalRequestsCreated = allRequests.Count,
            TotalResponsesMade = allResponses.Count,
            TotalAttachmentsAdded = userAuditLogs.Count(al => al.Action == RequestAuditAction.AttachmentAdded),
            TotalViews = userAuditLogs.Count(al => al.Action == RequestAuditAction.Viewed),
            UniqueActiveUsers = userAuditLogs.Select(al => al.UserId).Distinct().Count(),
            DailyBreakdown = dailyBreakdown,
            TopUsers = topUsers
        };
    }

    public async Task<Result<List<TransactionUserActivityItemDto>>> GetUserActivityReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var from = fromDate?.Date ?? DateTime.UtcNow.AddMonths(-1);
            var to = (toDate?.Date ?? DateTime.UtcNow).AddDays(1);

            var auditLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
                al => al.Timestamp >= from && al.Timestamp < to,
                transform: q => q.Include(al => al.Request!).AsNoTracking());

            var userLogs = auditLogs.IsError || auditLogs.Value is null
                ? []
                : auditLogs.Value.Where(al => al.Request != null && al.Request.RequesterId == currentUserId).ToList();

            var userGroups = userLogs.GroupBy(al => al.UserId);

            var result = userGroups.Select(g =>
            {
                return new TransactionUserActivityItemDto
                {
                    UserId = g.Key,
                    UserName = g.Key.ToString(),
                    RequestsCreated = g.Count(al => al.Action == RequestAuditAction.Created),
                    ResponsesMade = g.Count(al => al.Action == RequestAuditAction.Responded),
                    AttachmentsAdded = g.Count(al => al.Action == RequestAuditAction.AttachmentAdded),
                    TotalActions = g.Count(),
                    LastActivityDate = g.Max(al => al.Timestamp)
                };
            }).OrderByDescending(x => x.TotalActions).ToList();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user activity report");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<List<TransactionActiveUserItemDto>>> GetActiveUsersAsync(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var from = fromDate?.Date ?? DateTime.UtcNow.AddMonths(-1);
            var to = (toDate?.Date ?? DateTime.UtcNow).AddDays(1);

            var auditLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
                al => al.Timestamp >= from && al.Timestamp < to,
                transform: q => q.Include(al => al.Request!).AsNoTracking());

            var userLogs = auditLogs.IsError || auditLogs.Value is null
                ? []
                : auditLogs.Value.Where(al => al.Request != null && al.Request.RequesterId == currentUserId).ToList();

            var userGroups = userLogs.GroupBy(al => al.UserId);

            var result = userGroups.Select(g =>
            {
                return new TransactionActiveUserItemDto
                {
                    UserId = g.Key,
                    UserName = g.Key.ToString(),
                    TotalActions = g.Count(),
                    FirstActionDate = g.Min(al => al.Timestamp),
                    LastActionDate = g.Max(al => al.Timestamp),
                    ActionsPerformed = g.Select(al => al.Action.ToString()).Distinct().ToList()
                };
            }).OrderByDescending(x => x.TotalActions).ToList();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching active users report");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionStorageReportDto>> GetStorageReportAsync()
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var requestAttachments = await unitOfWork.RequestAttachments.GetAllAsync(
                transform: q => q.Include(ra => ra.Attachment!)
                    .Include(ra => ra.Request!).AsNoTracking());

            var responseAttachments = await unitOfWork.ResponseAttachments.GetAllAsync(
                transform: q => q.Include(ra => ra.Attachment!)
                    .Include(ra => ra.Response!).ThenInclude(r => r.Request!).AsNoTracking());

            var reqAttachments = requestAttachments.IsError || requestAttachments.Value is null
                ? [] : requestAttachments.Value
                    .Where(ra => ra.Request != null && ra.Request.RequesterId == currentUserId)
                    .Select(ra => ra.Attachment)
                    .Where(a => a != null)
                    .ToList();

            var respAttachments = responseAttachments.IsError || responseAttachments.Value is null
                ? [] : responseAttachments.Value
                    .Where(ra => ra.Response != null && ra.Response.Request != null
                        && ra.Response.Request.RequesterId == currentUserId)
                    .Select(ra => ra.Attachment)
                    .Where(a => a != null)
                    .ToList();

            var allAttachments = reqAttachments.Concat(respAttachments).Cast<Attachment>().ToList();
            var totalBytes = allAttachments.Sum(a => a.Size);
            var totalFiles = allAttachments.Count;

            var perUser = allAttachments
                .GroupBy(a => a.CreatedByUserId)
                .Select(g =>
                {
                    var userId = Guid.TryParse(g.Key, out var uid) ? uid : Guid.Empty;
                    return new TransactionStoragePerUserDto
                    {
                        UserId = userId,
                        UserName = userId.ToString(),
                        TotalFiles = g.Count(),
                        TotalBytes = g.Sum(a => a.Size),
                        PercentageOfTotal = totalBytes > 0 ? Math.Round((double)g.Sum(a => a.Size) / totalBytes * 100, 2) : 0,
                        LastFileAddedAt = g.Max(a => a.CreatedAt)
                    };
                }).OrderByDescending(x => x.TotalBytes).ToList();

            var fileTypeBreakdown = allAttachments
                .GroupBy(a => a.Extension)
                .Select(g => new StoragePerTypeDto
                {
                    FileType = g.Key ?? "unknown",
                    Count = g.Count(),
                    TotalBytes = g.Sum(a => a.Size)
                }).OrderByDescending(x => x.TotalBytes).ToList();

            return new TransactionStorageReportDto
            {
                TotalStorageBytes = totalBytes,
                TotalFiles = totalFiles,
                PerUser = perUser,
                FileTypeBreakdown = fileTypeBreakdown
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching storage report");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionChartsDataDto>> GetChartsDataAsync(DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var from = fromDate?.Date ?? DateTime.UtcNow.AddDays(-30);
            var to = (toDate?.Date ?? DateTime.UtcNow).AddDays(1);

            var auditLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
                al => al.Timestamp >= from && al.Timestamp < to,
                transform: q => q.Include(al => al.Request!).AsNoTracking());

            var userLogs = auditLogs.IsError || auditLogs.Value is null
                ? []
                : auditLogs.Value.Where(al => al.Request != null && al.Request.RequesterId == currentUserId).ToList();

            var dailyActivity = userLogs
                .GroupBy(al => al.Timestamp.Date)
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Value = g.Count()
                }).OrderBy(x => x.Label).ToList();

            var actionTypeBreakdown = userLogs
                .GroupBy(al => al.Action.ToString())
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key,
                    Value = g.Count()
                }).ToList();

            var hourlyDistribution = Enumerable.Range(0, 24).Select(hour =>
            {
                var count = userLogs.Count(al => al.Timestamp.Hour == hour);
                return new ChartDataPointDto
                {
                    Label = $"{hour:D2}:00",
                    Value = count
                };
            }).ToList();

            var topActiveUsers = userLogs
                .GroupBy(al => al.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .Select(x => new ChartDataPointDto
                {
                    Label = x.UserId.ToString(),
                    Value = x.Count
                }).ToList();

            var storageReport = await GetStorageReportAsync();
            var topStorageUsers = !storageReport.IsError
                ? storageReport.Value!.PerUser
                    .OrderByDescending(x => x.TotalBytes)
                    .Take(10)
                    .Select(x => new ChartDataPointDto
                    {
                        Label = x.UserName,
                        Value = x.TotalBytes
                    }).ToList()
                : [];

            var trend7Days = userLogs
                .Where(al => al.Timestamp >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(al => al.Timestamp.Date)
                .Select(g => new ChartDataPointDto
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Value = g.Count()
                }).OrderBy(x => x.Label).ToList();

            return new TransactionChartsDataDto
            {
                DailyActivity = dailyActivity,
                ActionTypeBreakdown = actionTypeBreakdown,
                HourlyDistribution = hourlyDistribution,
                TopActiveUsers = topActiveUsers,
                TopStorageUsers = topStorageUsers,
                Trend7Days = trend7Days
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching charts data");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<TransactionDailyWorkDto>> GetDailyWorkReportAsync(DateTime? date)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var reportDate = (date ?? DateTime.UtcNow).Date;
            var dayEnd = reportDate.AddDays(1);

            var auditLogs = await unitOfWork.RequestAuditLogs.GetAllAsync(
                al => al.Timestamp >= reportDate && al.Timestamp < dayEnd,
                transform: q => q.Include(al => al.Request!).AsNoTracking());

            var userLogs = auditLogs.IsError || auditLogs.Value is null
                ? []
                : auditLogs.Value.Where(al => al.Request != null && al.Request.RequesterId == currentUserId).ToList();

            var auditItems = userLogs.Select(al => new TransactionDailyWorkAuditLogItemDto
            {
                Id = al.Id,
                RequestId = al.RequestId,
                RequestNumber = al.Request?.RequestNumber,
                UserName = al.UserId.ToString(),
                Action = al.Action.ToString(),
                Details = al.Details,
                Timestamp = al.Timestamp
            }).OrderByDescending(x => x.Timestamp).ToList();

            var requests = await unitOfWork.Requests.GetAllAsync(
                r => r.RequesterId == currentUserId
                    && r.CreatedAt.HasValue
                    && r.CreatedAt.Value >= reportDate
                    && r.CreatedAt.Value < dayEnd,
                transform: q => q.Include(r => r.RequestTemplateValues!)
                        .ThenInclude(rv => rv.Template)
                    .Include(r => r.RequestTemplateValues!)
                        .ThenInclude(rv => rv.InputValues).AsNoTracking());

            var requestItems = (requests.IsError ? [] : requests.Value!).Select(r =>
            {
                var template = r.RequestTemplateValues?.Template;
                var inputValues = r.RequestTemplateValues?.InputValues
                    .Select(iv => new DailyWorkFormValueItemDto { Key = iv.Key, Value = iv.Value })
                    .ToList() ?? [];

                return new TransactionDailyWorkRequestItemDto
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    TemplateName = template?.TemplateName,
                    RequesterName = r.RequesterId.ToString(),
                    Status = r.Status,
                    CreatedAt = r.CreatedAt ?? reportDate,
                    UpdatedAt = r.UpdatedAt,
                    FormValues = inputValues
                };
            }).ToList();

            return new TransactionDailyWorkDto
            {
                Date = reportDate,
                AuditLogs = auditItems,
                Requests = requestItems
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching daily work report");
            return TransactionErrors.InternalServerError;
        }
    }

    private static DateTime StartOfWeek(DateTime dt)
    {
        var diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dt.AddDays(-diff).Date;
    }
}
