namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/archive-report")]
[Authorize]
public class ArchiveReportController(
    IArchiveRecordReportService reportService,
    ILogger<ArchiveReportController> logger) : ControllerBase
{
    [HttpGet("my-departments")]
    public async Task<IActionResult> GetMyDepartments()
    {
        logger.LogInformation("Getting archive leader departments for current user");
        var result = await reportService.GetMyDepartmentsAsync();
        return result.ToActionResult();
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        logger.LogInformation("Getting dashboard for current user");
        var result = await reportService.GetDepartmentDashboardAsync();
        return result.ToActionResult();
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
    {
        logger.LogInformation("Getting daily report");
        var result = await reportService.GetDailyReportAsync(date);
        return result.ToActionResult();
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyReport([FromQuery] DateTime? weekStart)
    {
        logger.LogInformation("Getting weekly report");
        var result = await reportService.GetWeeklyReportAsync(weekStart);
        return result.ToActionResult();
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int? year, [FromQuery] int? month)
    {
        logger.LogInformation("Getting monthly report");
        var result = await reportService.GetMonthlyReportAsync(year, month);
        return result.ToActionResult();
    }

    [HttpGet("user-activity")]
    public async Task<IActionResult> GetUserActivity(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        logger.LogInformation("Getting user activity report");
        var result = await reportService.GetUserActivityReportAsync(fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("active-users")]
    public async Task<IActionResult> GetActiveUsers(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        logger.LogInformation("Getting active users");
        var result = await reportService.GetActiveUsersAsync(fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("storage")]
    public async Task<IActionResult> GetStorageReport()
    {
        logger.LogInformation("Getting storage report");
        var result = await reportService.GetStorageConsumptionReportAsync();
        return result.ToActionResult();
    }

    [HttpGet("charts")]
    public async Task<IActionResult> GetChartsData(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        logger.LogInformation("Getting charts data");
        var result = await reportService.GetChartsDataAsync(fromDate, toDate);
        return result.ToActionResult();
    }
}
