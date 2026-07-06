using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Transaction.Api.Extensions;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Api.Controllers;

[ApiController]
[Route("api/transaction/[controller]")]
[Authorize]
public class ReportsController(IReportService reportService, ILogger<ReportsController> logger) : ControllerBase
{
    [HttpGet("requests")]
    [EndpointPermission("reports.get-requests-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetRequestsReportPaged(int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate, bool forCurrentDepartment = false)
    {
        logger.LogInformation("Getting requests report, page: {Page}, size: {PageSize}, startDate: {StartDate}, endDate: {EndDate}, forCurrentDepartment: {ForCurrentDepartment}",
            pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
        var result = await reportService.GetRequestsReportPaged(pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
        return result.ToActionResult();
    }

    [HttpGet("responses")]
    [EndpointPermission("reports.get-responses-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetResponsesReportPaged(int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate, bool forCurrentDepartment = false)
    {
        logger.LogInformation("Getting responses report, page: {Page}, size: {PageSize}, startDate: {StartDate}, endDate: {EndDate}, forCurrentDepartment: {ForCurrentDepartment}",
            pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
        var result = await reportService.GetResponsesReportPaged(pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
        return result.ToActionResult();
    }

    [HttpGet("dashboard")]
    [EndpointPermission("reports.get-dashboard", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetDashboard()
    {
        logger.LogInformation("Getting transaction dashboard report");
        var result = await reportService.GetDashboardAsync();
        return result.ToActionResult();
    }

    [HttpGet("daily")]
    [EndpointPermission("reports.get-daily-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetDailyReport(DateTime? date)
    {
        logger.LogInformation("Getting daily report, date: {Date}", date);
        var result = await reportService.GetDailyReportAsync(date);
        return result.ToActionResult();
    }

    [HttpGet("weekly")]
    [EndpointPermission("reports.get-weekly-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetWeeklyReport(DateTime? weekStart)
    {
        logger.LogInformation("Getting weekly report, weekStart: {WeekStart}", weekStart);
        var result = await reportService.GetWeeklyReportAsync(weekStart);
        return result.ToActionResult();
    }

    [HttpGet("monthly")]
    [EndpointPermission("reports.get-monthly-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetMonthlyReport(int? year, int? month)
    {
        logger.LogInformation("Getting monthly report, year: {Year}, month: {Month}", year, month);
        var result = await reportService.GetMonthlyReportAsync(year, month);
        return result.ToActionResult();
    }

    [HttpGet("user-activity")]
    [EndpointPermission("reports.get-user-activity", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetUserActivityReport(DateTime? fromDate, DateTime? toDate)
    {
        logger.LogInformation("Getting user activity report, from: {From}, to: {To}", fromDate, toDate);
        var result = await reportService.GetUserActivityReportAsync(fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("active-users")]
    [EndpointPermission("reports.get-active-users", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetActiveUsers(DateTime? fromDate, DateTime? toDate)
    {
        logger.LogInformation("Getting active users report, from: {From}, to: {To}", fromDate, toDate);
        var result = await reportService.GetActiveUsersAsync(fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("storage")]
    [EndpointPermission("reports.get-storage-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetStorageReport()
    {
        logger.LogInformation("Getting storage report");
        var result = await reportService.GetStorageReportAsync();
        return result.ToActionResult();
    }

    [HttpGet("charts")]
    [EndpointPermission("reports.get-charts-data", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetChartsData(DateTime? fromDate, DateTime? toDate)
    {
        logger.LogInformation("Getting charts data, from: {From}, to: {To}", fromDate, toDate);
        var result = await reportService.GetChartsDataAsync(fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("daily-work")]
    [EndpointPermission("reports.get-daily-work-report", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetDailyWorkReport(DateTime? date)
    {
        logger.LogInformation("Getting daily work report, date: {Date}", date);
        var result = await reportService.GetDailyWorkReportAsync(date);
        return result.ToActionResult();
    }
}
