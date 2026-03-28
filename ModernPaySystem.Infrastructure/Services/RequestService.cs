using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Infrastructure.Services;

public class RequestService(
    IUnitOfWork unitOfWork, ILogger<RequestService> logger,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager) : IRequestService
{
    public async Task<Result<IEnumerable<RequestDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all requests");
            var requests = await unitOfWork.Requests.GetAllAsync(
                null,
                x => x.Include(x => x.Template)
                     .Include(x => x.Approver)
                     .Include(x => x.Requester)
                     .Include(x => x.RequestAttachments));
            if (requests.IsError)
                return requests.Errors;

            var requestDtos = requests.Value!.Select(r => r.ToDto()).ToList();
            return requestDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all requests");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<RequestDto>>> GetAllAsync(bool hasResponse)
    {
        try
        {
            logger.LogInformation("Fetching all requests");
            var requests = await unitOfWork.Requests.GetAllAsync(
                x => x.ResponseId.HasValue == hasResponse,
                x => x.Include(x => x.Template)
                      .Include(x => x.Response)
                     .Include(x => x.Approver)
                     .Include(x => x.Requester)
                     .Include(x => x.RequestAttachments));

            if (requests.IsError)
                return requests.Errors;

            return requests.Value!.ConvertAll(r => r.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all requests");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged requests, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(page, pageSize);
            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching request by id: {RequestId}", id);
            var request = await unitOfWork.Requests.GetByIdAsync(id);

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.RequestNotFound;

            return request.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request by id: {RequestId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<RequestDto>>> GetByRequesterIdAsync(Guid requesterId)
    {
        try
        {
            logger.LogInformation("Fetching requests for requester: {RequesterId}", requesterId);
            var requests = await unitOfWork.Requests.GetAllAsync(r => r.RequesterId == requesterId);
            if (requests.IsError)
                return requests.Errors;

            var requesterRequests = requests.Value!.ConvertAll(r => r.ToDto());
            return requesterRequests;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests for requester: {RequesterId}", requesterId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<RequestDto>>> GetByApproverIdAsync(Guid approverId)
    {
        try
        {
            logger.LogInformation("Fetching requests for approver: {ApproverId}", approverId);

            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            return requests.Value!.Where(r => r.ApproverId == approverId).Select(r => r.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests for approver: {ApproverId}", approverId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<RequestDto>>> GetByTemplateIdAsync(Guid templateId)
    {
        try
        {
            logger.LogInformation("Fetching requests for template: {TemplateId}", templateId);

            var requests = await unitOfWork.Requests.GetAllAsync(r => r.TemplateId == templateId);

            if (requests.IsError)
                return requests.Errors;

            return requests.Value!.ConvertAll(r => r.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests for template: {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestDto>> CreateAsync(CreateRequestDto request, List<IFormFile> files)
    {
        try
        {
            if (request == null)
                return ApplicationErrors.InvalidInput;

            if (request.TemplateId == Guid.Empty || request.ApproverId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Creating new request for requester: {RequesterId}", httpContextServiceManager.GetCurrentUserId());
            var requestEntity = new Request
            {
                TemplateId = request.TemplateId,
                RequesterId = httpContextServiceManager.GetCurrentUserId(),
                ApproverId = request.ApproverId,
                ContentAsJson = request.Content
            };

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

            var request = await unitOfWork.Requests.GetByIdAsync(id);
            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.RequestNotFound;

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

            var request = await unitOfWork.Requests.GetByIdAsync(requestId);
            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.RequestNotFound;

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

            var updatedRequest = await unitOfWork.Requests.GetByIdAsync(requestId);
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


    public async Task<Result<PagedList<RequestDto>>> GetAllRequestNeedActionPagedAsync(int page, int pageSize, bool hasResponse)
    {
        try
        {
            logger.LogInformation("Fetching paged requests with hasResponse filter, page: {Page}, size: {PageSize}, hasResponse: {HasResponse}", page, pageSize, hasResponse);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;

            var requestBuilder = new ExpressionBuilder<Request>();

            requestBuilder.And(r => r.ApproverId == httpContextServiceManager.GetCurrentUserId());
            requestBuilder.And(r => r.ResponseId.HasValue == hasResponse);

            var expression = requestBuilder.Build();

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(
                page,
                pageSize,
                expression,
                i => i.Include(x => x.RequestAttachments).ThenInclude(x => x.Attachment)!);

            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRequestDtos = new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);

            return pagedRequestDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests with hasResponse filter, page: {Page}, size: {PageSize}, hasResponse: {HasResponse}", page, pageSize, hasResponse);
            return ApplicationErrors.InternalServerError;
        }
    }
}
