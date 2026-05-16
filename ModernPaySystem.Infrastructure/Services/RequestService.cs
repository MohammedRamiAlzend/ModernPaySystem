using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ExpressionBuilderLib.src.Core;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace ModernPaySystem.Infrastructure.Services;


public class RequestService(
    IUnitOfWork unitOfWork, ILogger<RequestService> logger,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto = null)
    {
        try
        {
            var page = filterDto?.Page ?? 1;
            var pageSize = filterDto?.PageSize ?? 10;

            logger.LogInformation("Fetching paged requests, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            // Build filter expressions
            List<Expression<Func<Request, bool>>> filters = new();
            if (filterDto != null)
            {
                if (filterDto.FromDate.HasValue)
                    filters.Add(r => r.CreatedAt >= filterDto.FromDate);
                if (filterDto.ToDate.HasValue)
                    filters.Add(r => r.CreatedAt <= filterDto.ToDate);
                if (filterDto.InputValueFilters != null && filterDto.InputValueFilters.Count != 0)
                {
                    foreach (var ivf in filterDto.InputValueFilters)
                    {
                        if (!string.IsNullOrWhiteSpace(ivf.Value))
                        {
                            filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key.Contains(ivf.Key) && iv.Value.Contains(ivf.Value)));
                            //filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key == ivf.Key && iv.Value == ivf.Value));
                        }
                        else
                        {
                            filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key.Contains(ivf.Key)));
                            //filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key == ivf.Key));
                        }
                    }
                }
            }
            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page, pageSize,
                transform: i => i.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: filters
            //logicalOperator: filterDto?.LogicalOperator == FilterLogicalOperator.Or ? LogicalOperator.Or : LogicalOperator.And
            );
            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests, page: {Page}, size: {PageSize}", filterDto?.Page, filterDto?.PageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching request by id: {RequestId}", id);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var request = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == id,
                transform: x => x.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                 .Include(x => x.Approver)
                                 .Include(x => x.Requester)
                                 .Include(x => x.RequestAttachments),
                additionalFilters: new List<Expression<Func<Request, bool>>> { RequestExpressions.CanReadByUserId(currentUserId) });

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.UnauthorizedRequestAccess;

            return request.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request by id: {RequestId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetByRequesterIdAsync(Guid requesterId, RequestPagedFilterDto filterDto)
    {
        try
        {
            logger.LogInformation("Fetching paged requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, filterDto.Page, filterDto.PageSize);
            if (filterDto.Page <= 0)
                return ApplicationErrors.InvalidInput;
            if (filterDto.PageSize <= 0 || filterDto.PageSize > 100)
                return ApplicationErrors.InvalidInput;
            // Build filter expressions
            List<Expression<Func<Request, bool>>> filters = new();
            if (filterDto != null)
            {
                if (filterDto.FromDate.HasValue)
                    filters.Add(r => r.CreatedAt >= filterDto.FromDate);
                if (filterDto.ToDate.HasValue)
                    filters.Add(r => r.CreatedAt <= filterDto.ToDate);
                if (filterDto.InputValueFilters != null && filterDto.InputValueFilters.Count != 0)
                {
                    foreach (var ivf in filterDto.InputValueFilters)
                    {
                        if (!string.IsNullOrWhiteSpace(ivf.Value))
                        {
                            filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key.Contains(ivf.Key) && iv.Value.Contains(ivf.Value)));
                            //filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key == ivf.Key && iv.Value == ivf.Value));
                        }
                        else
                        {
                            filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key.Contains(ivf.Key)));
                            //filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key == ivf.Key));
                        }
                    }
                }
            }
            filters.AddRange(RequestExpressions.ByRequesterIdWithIncludes(requesterId));
            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                filterDto!.Page,
                filterDto.PageSize,
                transform: i => i.Include(r => r.RequestAttachments)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                .Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template),
                additionalFilters: filters);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, filterDto.Page, filterDto.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, filterDto.Page, filterDto.PageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetByApproverIdAsync(Guid approverId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged requests for approver: {ApproverId}, page: {Page}, size: {PageSize}", approverId, page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(r => r.RequestAttachments)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                .Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template),
                additionalFilters: new List<Expression<Func<Request, bool>>> { RequestExpressions.ByApproverId(approverId) });

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests for approver: {ApproverId}, page: {Page}, size: {PageSize}", approverId, page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetByTemplateIdAsync(Guid templateId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged requests for template: {TemplateId}, page: {Page}, size: {PageSize}", templateId, page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(r => r.RequestAttachments).Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template).Include(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: RequestExpressions.ByTemplateIdWithIncludes(templateId));

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests for template: {TemplateId}, page: {Page}, size: {PageSize}", templateId, page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files)
    {
        try
        {
            if (request == null)
                return ApplicationErrors.InvalidInput;

            if (request.TemplateId == Guid.Empty || request.DepartmentId == Guid.Empty)
                return ApplicationErrors.InvalidInput;
            var usersResult = await unitOfWork.Users.GetAllAsync(x => request.ReadOnlyUsers.Contains(x.Id));
            if (usersResult.IsError)
                return usersResult.Errors;

            logger.LogInformation("Creating new request for requester: {RequesterId}", httpContextServiceManager.GetCurrentUserId());
            var getDepartmentResult = await unitOfWork.Departments.GetAsync(x => x.Id == request.DepartmentId, i => i.Include(x => x.DepartmentHead));
            if (getDepartmentResult.IsError)
                return getDepartmentResult.Errors;

            // Add null check for DepartmentHeadId
            if (getDepartmentResult.Value?.DepartmentHeadId == null)
                return ApplicationErrors.DepartmentHeadIsNotSet;

            var getTemplateResult = await unitOfWork.Templates.GetByIdAsync(request.TemplateId);
            if (getTemplateResult.IsError)
                return getTemplateResult.Errors;
            if (getTemplateResult.Value!.IsRequireAttachments)
            {
                if (files == null || files.Count == 0)
                    return ApplicationErrors.MissingRequiredField;
            }
            var userResult = await unitOfWork.Users.GetByIdAsync(httpContextServiceManager.GetCurrentUserId());
            if (userResult.IsError)
            {
                return userResult.Errors;
            }
            var requestEntity = new Request
            {
                Id = Guid.NewGuid(),
                RequesterId = httpContextServiceManager.GetCurrentUserId(),
                ApproverId = getDepartmentResult.Value!.DepartmentHeadId.Value,
                ReadOnlyUsers = usersResult.Value,
                ApproverDepartmentId = getDepartmentResult.Value.Id,
                RequesterDepartmentId = userResult.Value!.DepartmentId!.Value
            };

            var newRequestTemplateValues = new RequestTemplateValues
            {
                Id = Guid.NewGuid(),
                TemplateId = request.TemplateId,
                RequestId = requestEntity.Id,
                InputValues = request.Content.Select(iv => new InputValue { Key = iv.Key, Value = iv.Value }).ToList()
            };

            // link the template values to the request entity
            requestEntity.RequestTemplateValuesId = newRequestTemplateValues.Id;

            var addRequestTemplateValuesResult = await unitOfWork.RequestTemplateValues.AddAsync(newRequestTemplateValues);
            if (addRequestTemplateValuesResult.IsError)
                return addRequestTemplateValuesResult.Errors;

            var addResult = await unitOfWork.Requests.AddAsync(requestEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;
            foreach (var file in files)
            {
                var uploadResult = await webAttachmentService.UploadFileToRequestAsync(file, requestEntity.Id);
                if (uploadResult.IsError)
                    return uploadResult.Errors;
            }

            logger.LogInformation("Successfully created request: {RequestId}", requestEntity.Id);
            return requestEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating request");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var request = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == id,
                additionalFilters: new List<Expression<Func<Request, bool>>> { RequestExpressions.CanMakeUpdateByUserId(currentUserId) });

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.UnauthorizedRequestAccess;

            logger.LogInformation("Deleting request: {RequestId}", id);

            await unitOfWork.Requests.RemoveAsync(x => x.Id == request.Value.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully deleted request: {RequestId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting request: {RequestId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> AddFilesToRequestAsync(Guid requestId, List<IFormFile> files)
    {
        try
        {
            if (requestId == Guid.Empty || files == null || !files.Any())
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var request = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == requestId,
                transform: x => x.Include(x => x.RequestAttachments),
                additionalFilters: new List<Expression<Func<Request, bool>>> { RequestExpressions.CanMakeUpdateByUserId(currentUserId) });

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.UnauthorizedRequestAccess;

            logger.LogInformation("Adding {FileCount} Files to request: {RequestId}", files.Count, requestId);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var uploadResult = await webAttachmentService.UploadFileToRequestAsync(file, request.Value.Id);
                    if (uploadResult.IsError)
                        return uploadResult.Errors;
                }
            }

            logger.LogInformation("Successfully added {FileCount} Files to request: {RequestId}", files.Count, requestId);

            var updatedRequest = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == requestId,
                transform: x => x.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                 .Include(x => x.Approver)
                                 .Include(x => x.Requester)
                                 .Include(x => x.RequestAttachments));

            if (updatedRequest.IsError)
                return updatedRequest.Errors;

            return updatedRequest.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding Files to request: {RequestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetAllRequestNeedActionPagedAsync(RequestPagedFilterDto? filterDto, bool hasResponse)
    {
        try
        {
            var page = filterDto?.Page ?? 1;
            var pageSize = filterDto?.PageSize ?? 10;
            logger.LogInformation("Fetching paged requests with hasResponse filter, page: {Page}, size: {PageSize}, hasResponse: {HasResponse}", page, pageSize, hasResponse);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            // Build filter expressions
            List<Expression<Func<Request, bool>>> filters = new();
            filters.AddRange(RequestExpressions.RequestsNeedAction(httpContextServiceManager.GetCurrentUserId(), hasResponse));
            if (filterDto != null)
            {
                if (filterDto.FromDate.HasValue)
                    filters.Add(r => r.CreatedAt >= filterDto.FromDate);
                if (filterDto.ToDate.HasValue)
                    filters.Add(r => r.CreatedAt <= filterDto.ToDate);
                if (filterDto.InputValueFilters != null && filterDto.InputValueFilters.Count != 0)
                {
                    foreach (var ivf in filterDto.InputValueFilters)
                    {
                        if (!string.IsNullOrWhiteSpace(ivf.Value))
                        {
                            filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key.Contains(ivf.Key) && iv.Value.Contains(ivf.Value)));
                        }
                        else
                        {
                            filters.Add(r => r.RequestTemplateValues != null && r.RequestTemplateValues.InputValues.Any(iv => iv.Key.Contains(ivf.Key)));
                        }
                    }
                }
            }
            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(x => x.RequestAttachments).ThenInclude(x => x.Attachment)!.Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: filters,
                logicalOperator: filterDto?.LogicalOperator == FilterLogicalOperator.Or ? ExpressionBuilderLib.src.Core.Enums.LogicalOperator.Or : ExpressionBuilderLib.src.Core.Enums.LogicalOperator.And);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRequestDtos = new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);

            return pagedRequestDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests with hasResponse filter, page: {Page}, size: {PageSize}, hasResponse: {HasResponse}", filterDto?.Page, filterDto?.PageSize, hasResponse);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetPendingByCurrentRequesterPagedAsync(int page, int pageSize)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Fetching pending requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", currentUserId, page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;

            var requestBuilder = new ExpressionBuilder<Request>();
            requestBuilder.And(r => r.RequesterId == currentUserId);
            requestBuilder.And(r => !r.ResponseId.HasValue);

            var expression = requestBuilder.Build();

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                expression,
                i => i.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)

                      .Include(x => x.Approver)
                      .Include(x => x.Requester)
                      .Include(x => x.RequestAttachments).ThenInclude(x => x.Attachment)!);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching pending requests for requester, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }
}
