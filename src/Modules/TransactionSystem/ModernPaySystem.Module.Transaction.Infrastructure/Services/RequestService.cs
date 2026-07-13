using ExpressionBuilderLib.src.Core;
using ExpressionBuilderLib.src.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.Module.Transaction.Domain.DTOs;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using SKIdepartmentService = ModernPaySystem.SharedKernel.Application.Interfaces.IDepartmentService;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;
using System.Linq.Expressions;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class RequestService(
    ITransactionUnitOfWork unitOfWork,
    ILogger<RequestService> logger,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager,
    SKIdepartmentService departmentService) : IRequestService
{
    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(RequestPagedFilterDto? filterDto = null)
    {
        try
        {
            var page = filterDto?.Page ?? 1;
            var pageSize = filterDto?.PageSize ?? 10;

            logger.LogInformation("Fetching paged requests, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            List<Expression<Func<Request, bool>>> filters = [];
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
                page, pageSize,
                transform: i => i.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest),
                additionalFilters: filters);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests, page: {Page}, size: {PageSize}", filterDto?.Page, filterDto?.PageSize);
            return TransactionErrors.InternalServerError;
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
                                 .Include(x => x.RequestAttachments)
                                 .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest)
                                 .Include(r => r.CurrentTransaction)
                                 .Include(r => r.FirstTransaction));

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return TransactionErrors.RequestNotFound;

            if (!request.Value.CanView(currentUserId))
                return TransactionErrors.UnauthorizedRequestAccess;

            return request.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request by id: {RequestId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetByRequesterIdAsync(Guid requesterId, RequestPagedFilterDto filterDto)
    {
        try
        {
            logger.LogInformation("Fetching paged requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, filterDto.Page, filterDto.PageSize);
            if (filterDto.Page <= 0)
                return TransactionErrors.InvalidInput;
            if (filterDto.PageSize <= 0 || filterDto.PageSize > 100)
                return TransactionErrors.InvalidInput;

            List<Expression<Func<Request, bool>>> filters = [];
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
            filters.AddRange(RequestExpressions.ByRequesterIdWithIncludes(requesterId));
            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                filterDto!.Page,
                filterDto.PageSize,
                transform: i => i.Include(r => r.RequestAttachments)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                 .Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                                 .Include(r => r.OutgoingRelations).ThenInclude(r => r.TargetRequest),
                additionalFilters: filters);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, filterDto.Page, filterDto.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, filterDto.Page, filterDto.PageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetByApproverIdAsync(Guid approverId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged requests for approver: {ApproverId}, page: {Page}, size: {PageSize}", approverId, page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(r => r.RequestAttachments)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                 .Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                 .Include(r => r.OutgoingRelations).ThenInclude(r => r.TargetRequest),
                additionalFilters: [RequestExpressions.ByApproverId(approverId)]);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests for approver: {ApproverId}, page: {Page}, size: {PageSize}", approverId, page, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetByTemplateIdAsync(Guid templateId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged requests for template: {TemplateId}, page: {Page}, size: {PageSize}", templateId, page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(r => r.RequestAttachments).Include(r => r.RequestTemplateValues).ThenInclude(x => x!.Template).Include(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues).Include(r => r.OutgoingRelations).ThenInclude(r => r.TargetRequest),
                additionalFilters: RequestExpressions.ByTemplateIdWithIncludes(templateId));

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests for template: {TemplateId}, page: {Page}, size: {PageSize}", templateId, page, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files)
    {
        try
        {
            if (request == null)
                return TransactionErrors.InvalidInput;

            if (request.TemplateId == Guid.Empty || request.DepartmentId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            logger.LogInformation("Creating new request for requester: {RequesterId}", httpContextServiceManager.GetCurrentUserId());

            var getTemplateResult = await unitOfWork.Templates.GetByIdAsync(request.TemplateId);
            if (getTemplateResult.IsError)
                return getTemplateResult.Errors;

            if (getTemplateResult.Value!.IsRequireAttachments)
            {
                if (files == null || files.Count == 0)
                    return TransactionErrors.MissingRequiredField;
            }

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var departmentResult = await departmentService.GetByIdAsync(request.DepartmentId);
            if (departmentResult.IsError)
                return departmentResult.Errors;
            if (departmentResult.Value == null)
                return TransactionErrors.InvalidInput;
            if (departmentResult.Value.DepartmentHeadId == Guid.Empty)
                return TransactionErrors.DepartmentHeadIsNotSet;

            var userDepartmentResult = await departmentService.GetByUserIdAsync(currentUserId);
            if (userDepartmentResult.IsError)
                return userDepartmentResult.Errors;

            var departmentTemplateNumberResult = await unitOfWork.DepartmentTemplateNumbers.GetAsync(
                dtn => dtn.DepartmentId == request.DepartmentId && dtn.TemplateId == request.TemplateId);

            DepartmentTemplateNumber departmentTemplateNumber;
            if (departmentTemplateNumberResult.IsError || departmentTemplateNumberResult.Value == null)
            {
                departmentTemplateNumber = new DepartmentTemplateNumber
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = request.DepartmentId,
                    TemplateId = request.TemplateId,
                    LastRequestNumber = 0
                };
                var addDtnResult = await unitOfWork.DepartmentTemplateNumbers.AddAsync(departmentTemplateNumber);
                if (addDtnResult.IsError)
                    return addDtnResult.Errors;
            }
            else
            {
                departmentTemplateNumber = departmentTemplateNumberResult.Value;
            }

            var requestEntity = new Request
            {
                Id = Guid.NewGuid(),
                RequestNumber = ++departmentTemplateNumber.LastRequestNumber,
                RequesterId = currentUserId,
                ApproverId = departmentResult.Value.DepartmentHeadId,
                ReadOnlyUsers = request.ReadOnlyUsers,
                ApproverDepartmentId = departmentResult.Value.Id,
                RequesterDepartmentId = userDepartmentResult.Value!.Id
            };

            var newRequestTemplateValues = new RequestTemplateValues
            {
                Id = Guid.NewGuid(),
                TemplateId = request.TemplateId,
                RequestId = requestEntity.Id,
                Request = requestEntity,
                InputValues = [.. request.Content.Select(iv => new InputValue { Key = iv.Key, Value = iv.Value })]
            };

            requestEntity.RequestTemplateValuesId = newRequestTemplateValues.Id;
            requestEntity.RequestTemplateValues = newRequestTemplateValues;

            var relationKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (request.RelatedRequests is not null)
            {
                foreach (var relatedRequest in request.RelatedRequests)
                {
                    if (relatedRequest.TargetRequestId == Guid.Empty)
                        return TransactionErrors.InvalidInput;

                    var relationKey = $"{relatedRequest.TargetRequestId}:{(int)relatedRequest.RelationType}";
                    if (!relationKeys.Add(relationKey))
                        return TransactionErrors.DuplicateRelation;

                    var targetRequestResult = await unitOfWork.Requests.GetByIdAsync(relatedRequest.TargetRequestId);
                    if (targetRequestResult.IsError)
                        return targetRequestResult.Errors;

                    if (targetRequestResult.Value == null)
                        return TransactionErrors.RequestNotFound;

                    var relation = new RequestRelation
                    {
                        Id = Guid.NewGuid(),
                        SourceRequestId = requestEntity.Id,
                        TargetRequestId = relatedRequest.TargetRequestId,
                        RelationType = relatedRequest.RelationType,
                        Notes = relatedRequest.Notes,
                        CreatedByUserId = currentUserId.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedByUserId = currentUserId.ToString(),
                        UpdatedAt = DateTime.UtcNow
                    };

                    var addRelationResult = await unitOfWork.RequestRelations.AddAsync(relation);
                    if (addRelationResult.IsError)
                    {
                        if (unitOfWork.HasActiveTransaction)
                            await unitOfWork.RollbackTransactionAsync();
                        return addRelationResult.Errors;
                    }
                }
            }

            var addRequestTemplateValuesResult = await unitOfWork.RequestTemplateValues.AddAsync(newRequestTemplateValues);
            if (addRequestTemplateValuesResult.IsError)
            {
                if (unitOfWork.HasActiveTransaction)
                    await unitOfWork.RollbackTransactionAsync();
                return addRequestTemplateValuesResult.Errors;
            }

            var addResult = await unitOfWork.Requests.AddAsync(requestEntity);
            if (addResult.IsError)
            {
                if (unitOfWork.HasActiveTransaction)
                    await unitOfWork.RollbackTransactionAsync();
                return addResult.Errors;
            }

            await unitOfWork.SaveChangesAsync();

            foreach (var file in files)
            {
                var uploadResult = await webAttachmentService.UploadFileToRequestAsync(file, requestEntity.Id);
                if (uploadResult.IsError)
                    return uploadResult.Errors;
            }

            logger.LogInformation("Successfully created request: {RequestId} with number {RequestNumber}", requestEntity.Id, requestEntity.RequestNumber);
            var createdRequest = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == requestEntity.Id,
                transform: x => x.Include(x => x.RequestTemplateValues).ThenInclude(x => x!.Template)
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                 .Include(x => x.RequestAttachments)
                                 .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest));

            if (createdRequest.IsError)
                return createdRequest.Errors;

            if (createdRequest.Value == null)
                return TransactionErrors.RequestNotFound;

            return createdRequest.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating request");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var request = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == id,
                additionalFilters: [RequestExpressions.CanMakeUpdateByUserId(currentUserId)]);

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return TransactionErrors.UnauthorizedDeleteRequest;

            logger.LogInformation("Deleting request: {RequestId}", id);

            await unitOfWork.Requests.RemoveAsync(x => x.Id == request.Value.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return TransactionErrors.DatabaseError;

            logger.LogInformation("Successfully deleted request: {RequestId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting request: {RequestId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> AddFilesToRequestAsync(Guid requestId, List<IFormFile> files)
    {
        try
        {
            if (requestId == Guid.Empty || files == null || !files.Any())
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var request = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == requestId,
                transform: x => x.Include(x => x.RequestAttachments),
                additionalFilters: [RequestExpressions.CanMakeUpdateByUserId(currentUserId)]);

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return TransactionErrors.UnauthorizedModifyRequest;

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
                                 .Include(x => x.RequestAttachments)
                                 .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest));

            if (updatedRequest.IsError)
                return updatedRequest.Errors;

            return updatedRequest.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding Files to request: {RequestId}", requestId);
            return TransactionErrors.InternalServerError;
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
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            List<Expression<Func<Request, bool>>> filters = [];
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
                                 .Include(x => x.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                                 .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest),
                additionalFilters: filters,
                logicalOperator: filterDto?.LogicalOperator == FilterLogicalOperator.Or ? LogicalOperator.Or : LogicalOperator.And);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRequestDtos = new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);

            return pagedRequestDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests with hasResponse filter, page: {Page}, size: {PageSize}, hasResponse: {HasResponse}", filterDto?.Page, filterDto?.PageSize, hasResponse);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetPendingByCurrentRequesterPagedAsync(int page, int pageSize)
    {
        try
        {
            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Fetching pending requests for requester: {RequesterId}, page: {Page}, size: {PageSize}", currentUserId, page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;

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

                       .Include(x => x.RequestAttachments).ThenInclude(x => x.Attachment)!
                       .Include(x => x.OutgoingRelations).ThenInclude(r => r.TargetRequest)!);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching pending requests for requester, page: {Page}, size: {PageSize}", page, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<List<RequestRelationDto>>> GetRelationsByRequestIdAsync(Guid requestId)
    {
        try
        {
            if (requestId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var request = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == requestId,
                transform: q => q.Include(r => r.CurrentTransaction).Include(r => r.FirstTransaction));

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return TransactionErrors.RequestNotFound;

            if (!request.Value.CanView(currentUserId))
                return TransactionErrors.UnauthorizedRequestRelationAccess;

            var relations = await unitOfWork.RequestRelations.GetAllAsync(
                filter: r => r.SourceRequestId == requestId,
                transform: q => q.Include(r => r.SourceRequest).Include(r => r.TargetRequest));

            if (relations.IsError)
                return relations.Errors;

            return relations.Value!.Select(r => r.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request relations for request: {RequestId}", requestId);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestRelationDto>> GetRelationByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var relation = await unitOfWork.RequestRelations.GetAsync(
                filter: r => r.Id == id,
                transform: q => q.Include(r => r.SourceRequest).Include(r => r.TargetRequest));

            if (relation.IsError)
                return relation.Errors;

            if (relation.Value == null)
                return TransactionErrors.RequestRelationNotFound;

            var sourceAccess = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == relation.Value.SourceRequestId,
                transform: q => q.Include(r => r.CurrentTransaction).Include(r => r.FirstTransaction));

            if (sourceAccess.IsError)
                return sourceAccess.Errors;

            if (sourceAccess.Value == null)
                return TransactionErrors.RequestNotFound;

            if (!sourceAccess.Value.CanView(currentUserId))
                return TransactionErrors.UnauthorizedViewRelation;

            return relation.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request relation by id: {RelationId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestRelationDto>> CreateRelationAsync(CreateRequestRelationDto dto)
    {
        try
        {
            if (dto == null || dto.SourceRequestId == Guid.Empty || dto.TargetRequestId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            if (dto.SourceRequestId == dto.TargetRequestId)
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var sourceRequestExists = await unitOfWork.Requests.GetByIdAsync(dto.SourceRequestId);
            if (sourceRequestExists.IsError)
                return sourceRequestExists.Errors;
            if (sourceRequestExists.Value == null)
                return TransactionErrors.RequestNotFound;

            var sourceRequestAccess = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == dto.SourceRequestId,
                additionalFilters: [RequestExpressions.CanMakeUpdateByUserId(currentUserId)]);

            if (sourceRequestAccess.IsError)
                return sourceRequestAccess.Errors;
            if (sourceRequestAccess.Value == null)
                return TransactionErrors.UnauthorizedModifySourceRequest;

            var targetRequestExists = await unitOfWork.Requests.GetByIdAsync(dto.TargetRequestId);
            if (targetRequestExists.IsError)
                return targetRequestExists.Errors;
            if (targetRequestExists.Value == null)
                return TransactionErrors.RequestNotFound;

            var targetRequestAccess = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == dto.TargetRequestId,
                transform: q => q.Include(r => r.CurrentTransaction).Include(r => r.FirstTransaction));

            if (targetRequestAccess.IsError)
                return targetRequestAccess.Errors;
            if (targetRequestAccess.Value == null)
                return TransactionErrors.RequestNotFound;

            if (!targetRequestAccess.Value.CanView(currentUserId))
                return TransactionErrors.UnauthorizedViewTargetRequest;

            var duplicateExists = await unitOfWork.RequestRelations.AnyAsync(
                r => r.SourceRequestId == dto.SourceRequestId
                     && r.TargetRequestId == dto.TargetRequestId
                     && r.RelationType == dto.RelationType);

            if (duplicateExists)
                return TransactionErrors.RequestRelationAlreadyExists;

            var relation = new RequestRelation
            {
                Id = Guid.NewGuid(),
                SourceRequestId = dto.SourceRequestId,
                TargetRequestId = dto.TargetRequestId,
                RelationType = dto.RelationType,
                Notes = dto.Notes,
                CreatedByUserId = currentUserId.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedByUserId = currentUserId.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            var addResult = await unitOfWork.RequestRelations.AddAsync(relation);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            var createdRelation = await unitOfWork.RequestRelations.GetAsync(
                filter: r => r.Id == relation.Id,
                transform: q => q.Include(r => r.SourceRequest).Include(r => r.TargetRequest));

            if (createdRelation.IsError)
                return createdRelation.Errors;

            return createdRelation.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating request relation");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestRelationDto>> UpdateRelationAsync(Guid id, UpdateRequestRelationDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || dto.SourceRequestId == Guid.Empty || dto.TargetRequestId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            if (dto.SourceRequestId == dto.TargetRequestId)
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var existingRelation = await unitOfWork.RequestRelations.GetAsync(
                filter: r => r.Id == id,
                transform: q => q.Include(r => r.SourceRequest).Include(r => r.TargetRequest));

            if (existingRelation.IsError)
                return existingRelation.Errors;

            if (existingRelation.Value == null)
                return TransactionErrors.RequestRelationNotFound;

            var currentSourceAccess = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == existingRelation.Value.SourceRequestId,
                additionalFilters: [RequestExpressions.CanMakeUpdateByUserId(currentUserId)]);

            if (currentSourceAccess.IsError)
                return currentSourceAccess.Errors;
            if (currentSourceAccess.Value == null)
                return TransactionErrors.UnauthorizedModifySourceRequest;

            if (existingRelation.Value.SourceRequestId != dto.SourceRequestId)
            {
                var newSourceExists = await unitOfWork.Requests.GetByIdAsync(dto.SourceRequestId);
                if (newSourceExists.IsError)
                    return newSourceExists.Errors;
                if (newSourceExists.Value == null)
                    return TransactionErrors.RequestNotFound;

                var newSourceAccess = await unitOfWork.Requests.GetAsync(
                    filter: r => r.Id == dto.SourceRequestId,
                    additionalFilters: [RequestExpressions.CanMakeUpdateByUserId(currentUserId)]);

                if (newSourceAccess.IsError)
                    return newSourceAccess.Errors;
                if (newSourceAccess.Value == null)
                    return TransactionErrors.UnauthorizedModifyNewSourceRequest;
            }

            var targetExists = await unitOfWork.Requests.GetByIdAsync(dto.TargetRequestId);
            if (targetExists.IsError)
                return targetExists.Errors;
            if (targetExists.Value == null)
                return TransactionErrors.RequestNotFound;

            var targetAccess = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == dto.TargetRequestId,
                transform: q => q.Include(r => r.CurrentTransaction).Include(r => r.FirstTransaction));

            if (targetAccess.IsError)
                return targetAccess.Errors;
            if (targetAccess.Value == null)
                return TransactionErrors.RequestNotFound;

            if (!targetAccess.Value.CanView(currentUserId))
                return TransactionErrors.UnauthorizedViewTargetRequest;

            var duplicateExists = await unitOfWork.RequestRelations.AnyAsync(
                r => r.Id != id
                     && r.SourceRequestId == dto.SourceRequestId
                     && r.TargetRequestId == dto.TargetRequestId
                     && r.RelationType == dto.RelationType);

            if (duplicateExists)
                return TransactionErrors.RequestRelationAlreadyExists;

            existingRelation.Value.SourceRequestId = dto.SourceRequestId;
            existingRelation.Value.TargetRequestId = dto.TargetRequestId;
            existingRelation.Value.RelationType = dto.RelationType;
            existingRelation.Value.Notes = dto.Notes;
            existingRelation.Value.UpdatedByUserId = currentUserId.ToString();
            existingRelation.Value.UpdatedAt = DateTime.UtcNow;

            var updateResult = await unitOfWork.RequestRelations.UpdateAsync(existingRelation.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            var updatedRelation = await unitOfWork.RequestRelations.GetAsync(
                filter: r => r.Id == id,
                transform: q => q.Include(r => r.SourceRequest).Include(r => r.TargetRequest));

            if (updatedRelation.IsError)
                return updatedRelation.Errors;

            return updatedRelation.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating request relation: {RelationId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteRelationAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var relation = await unitOfWork.RequestRelations.GetAsync(
                filter: r => r.Id == id,
                transform: q => q.Include(r => r.SourceRequest));

            if (relation.IsError)
                return relation.Errors;

            if (relation.Value == null)
                return TransactionErrors.RequestRelationNotFound;

            var sourceAccess = await unitOfWork.Requests.GetAsync(
                filter: r => r.Id == relation.Value.SourceRequestId,
                additionalFilters: [RequestExpressions.CanMakeUpdateByUserId(currentUserId)]);

            if (sourceAccess.IsError)
                return sourceAccess.Errors;

            if (sourceAccess.Value == null)
                return TransactionErrors.UnauthorizedDeleteRelation;

            var deleteResult = await unitOfWork.RequestRelations.RemoveAsync(r => r.Id == id);
            if (deleteResult.IsError)
                return deleteResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting request relation: {RelationId}", id);
            return TransactionErrors.InternalServerError;
        }
    }
}
