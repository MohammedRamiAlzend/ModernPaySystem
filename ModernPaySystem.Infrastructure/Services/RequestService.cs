using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Request service CRUD operations.
/// </summary>
public class RequestService(IUnitOfWork unitOfWork, ILogger<RequestService> logger, IWebAttachmentService webAttachmentService, IHttpContextServiceManager httpContextServiceManager) : IRequestService
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

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(page, pageSize);
            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRequestDtos = new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);

            return pagedRequestDtos;
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
            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            var requesterRequests = requests.Value!.Where(r => r.RequesterId == requesterId).Select(r => r.ToDto()).ToList();
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

            var approverRequests = requests.Value!.Where(r => r.ApproverId == approverId).Select(r => r.ToDto()).ToList();
            return approverRequests;
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
            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;
            var templateRequests = requests.Value!.Where(r => r.TemplateId == templateId).Select(r => r.ToDto()).ToList();
            return templateRequests;
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

            if (request.TemplateId == Guid.Empty || request.RequesterId == Guid.Empty || request.ApproverId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Creating new request for requester: {RequesterId}", request.RequesterId);
            var requestEntity = new Request
            {
                TemplateId = request.TemplateId,
                RequesterId = request.RequesterId,
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

    public async Task<Result<RequestDto>> UpdateAsync(Guid id, UpdateRequestDto request)
    {
        try
        {
            if (id == Guid.Empty || request == null)
                return ApplicationErrors.InvalidInput;

            var existingRequest = await unitOfWork.Requests.GetByIdAsync(id);
            if (existingRequest.IsError)
                return existingRequest.Errors;

            if (existingRequest.Value == null)
                return ApplicationErrors.RequestNotFound;

            logger.LogInformation("Updating request: {RequestId}", id);

            existingRequest.Value.TemplateId = request.TemplateId;
            existingRequest.Value.RequesterId = request.RequesterId;
            existingRequest.Value.ApproverId = request.ApproverId;
            existingRequest.Value.ContentAsJson = request.Content;

            await unitOfWork.Requests.UpdateAsync(existingRequest.Value);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully updated request: {RequestId}", id);
            return existingRequest.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating request: {RequestId}", id);
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

            // Verify the request exists
            var request = await unitOfWork.Requests.GetByIdAsync(requestId);
            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.RequestNotFound;

            logger.LogInformation("Adding {FileCount} Files to request: {RequestId}", files.Count, requestId);

            // Process each file and associate it with the request using WebAttachmentService
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

            // Return the updated request with its attachments
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

    public async Task<Result<IEnumerable<RequestDto>>> GetReceivedRequestsAsync()
    {
        try
        {
            logger.LogInformation("Fetching requests received by current user");

            // Get the current user ID from the HTTP context
            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            // Create an expression builder for Request entities
            var requestBuilder = new ExpressionBuilder<Request>();

            // Add a condition to filter requests where the ApproverId matches the current user ID
            // This represents requests that were sent TO the current user (they are the approver)
            requestBuilder.And(r => r.ApproverId == currentUserId);

            // Build the expression
            var expression = requestBuilder.Build();

            // Get requests that match the expression
            var requests = await unitOfWork.Requests.FindAsync(expression);
            if (requests.IsError)
                return requests.Errors;

            var requestDtos = requests.Value!.Select(r => r.ToDto()).ToList();
            return requestDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests received by current user");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RequestDto>>> GetReceivedRequestsPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged requests received by current user, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            // Get the current user ID from the HTTP context
            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            // Create an expression builder for Request entities
            var requestBuilder = new ExpressionBuilder<Request>();

            // Add a condition to filter requests where the ApproverId matches the current user ID
            // This represents requests that were sent TO the current user (they are the approver)
            requestBuilder.And(r => r.ApproverId == currentUserId);

            // Build the expression
            var expression = requestBuilder.Build();

            // Get requests that match the expression with pagination
            var pagedRequests = await unitOfWork.Requests.GetPagedAsync(page, pageSize, expression);
            if (pagedRequests.IsError)
                return pagedRequests.Errors;

            var requestDtos = pagedRequests.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRequestDtos = new PagedList<RequestDto>(requestDtos, pagedRequests.Value.TotalItems, page, pageSize);

            return pagedRequestDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests received by current user, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }
}
