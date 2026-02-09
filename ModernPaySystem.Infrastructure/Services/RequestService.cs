using FileManager.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Request service CRUD operations.
/// </summary>
public class RequestService(IUnitOfWork unitOfWork, ILogger<RequestService> logger, IFileManager fileManager) : IRequestService
{
    public async Task<Result<IEnumerable<Request>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all requests");
            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            return requests.Value!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all requests");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<Request>>> GetPagedAsync(int page, int pageSize)
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

            return pagedRequests.Value!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged requests, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Request>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching request by id: {RequestId}", id);
            var request = await unitOfWork.Requests.GetByIdAsync(id);

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.RequestNotFound;

            return request!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request by id: {RequestId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Request>>> GetByRequesterIdAsync(Guid requesterId)
    {
        try
        {
            logger.LogInformation("Fetching requests for requester: {RequesterId}", requesterId);
            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            var requesterRequests = requests.Value!.Where(r => r.RequesterId == requesterId).ToList();
            return requesterRequests;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests for requester: {RequesterId}", requesterId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Request>>> GetByApproverIdAsync(Guid approverId)
    {
        try
        {
            logger.LogInformation("Fetching requests for approver: {ApproverId}", approverId);
            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            var approverRequests = requests.Value!.Where(r => r.ApproverId == approverId).ToList();
            return approverRequests;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching requests for approver: {ApproverId}", approverId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Request>>> GetByTemplateIdAsync(Guid templateId)
    {
        try
        {
            logger.LogInformation("Fetching requests for template: {TemplateId}", templateId);
            var requests = await unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;
            var templateRequests = requests.Value!.Where(r => r.TemplateId == templateId).ToList();
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
            if (result < 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully created request: {RequestId}", requestEntity.Id);
            return requestEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating request");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Request>> UpdateAsync(Guid id, Request request)
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
            existingRequest.Value.ContentAsJson = request.ContentAsJson;

            await unitOfWork.Requests.UpdateAsync(existingRequest.Value);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully updated request: {RequestId}", id);
            return existingRequest!;
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
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully deleted request: {RequestId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting request: {RequestId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
}
