using ModernPaySystem.Application.Interfaces.TransactionSystemInterfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.DTOs;

namespace ModernPaySystem.Controllers.TransactionsSystemControllers;

[ApiController]
[Route("api/[controller]")]
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
}