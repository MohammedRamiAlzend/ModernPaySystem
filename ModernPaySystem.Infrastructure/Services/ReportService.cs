using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces.TransactionSystemInterfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using System.Linq.Expressions;

namespace ModernPaySystem.Infrastructure.Services;

public class ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger, IHttpContextServiceManager httpContextServiceManager) : IReportService
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
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            if (startDate.HasValue && !endDate.HasValue)
                endDate = DateTime.Now;

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                return Error.Validation("R001", "startDate must be earlier than endDate.");

            List<Expression<Func<Request, bool>>> filters = [];

            if (startDate.HasValue)
                filters.Add(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                filters.Add(r => r.CreatedAt <= endDate.Value);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var userResults = await unitOfWork.Users.GetAsync(x => x.Id == currentUserId);
            if (forCurrentDepartment)
            {
                filters.Add(r => r.Requester != null && r.RequesterDepartmentId == userResults.Value!.DepartmentId);
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
            return ApplicationErrors.InternalServerError;
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
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            if (startDate.HasValue && !endDate.HasValue)
                endDate = DateTime.Now;

            if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
                return Error.Validation("R002", "startDate must be earlier than endDate.");

            List<Expression<Func<Response, bool>>> filters = [];

            if (startDate.HasValue)
                filters.Add(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                filters.Add(r => r.CreatedAt <= endDate.Value);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var userResults = await unitOfWork.Users.GetAsync(x => x.Id == currentUserId);
            if (forCurrentDepartment)
            {
                filters.Add(r => r.Request!.Requester != null && r.Request!.RequesterDepartmentId == userResults.Value!.DepartmentId);
            }
            else
            {
                filters.Add(r => r.Request!.RequesterId == currentUserId);
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
            return ApplicationErrors.InternalServerError;
        }
    }


}
