using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Request service CRUD operations
/// </summary>
public class RequestService : IRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestService> _logger;

    public RequestService(IUnitOfWork unitOfWork, ILogger<RequestService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Request>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all requests");
            var requests = await _unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            return requests.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all requests");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Request>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching request by id: {RequestId}", id);
            var request = await _unitOfWork.Requests.GetByIdAsync(id);

            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationError.RequestNotFound;

            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching request by id: {RequestId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Request>>> GetByRequesterIdAsync(Guid requesterId)
    {
        try
        {
            _logger.LogInformation("Fetching requests for requester: {RequesterId}", requesterId);
            var requests = await _unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            var requesterRequests = requests.Value.Where(r => r.RequesterId == requesterId).ToList();
            return requesterRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching requests for requester: {RequesterId}", requesterId);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Request>>> GetByApproverIdAsync(Guid approverId)
    {
        try
        {
            _logger.LogInformation("Fetching requests for approver: {ApproverId}", approverId);
            var requests = await _unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;

            var approverRequests = requests.Value.Where(r => r.ApproverId == approverId).ToList();
            return approverRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching requests for approver: {ApproverId}", approverId);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Request>>> GetByTemplateIdAsync(Guid templateId)
    {
        try
        {
            _logger.LogInformation("Fetching requests for template: {TemplateId}", templateId);
            var requests = await _unitOfWork.Requests.GetAllAsync();
            if (requests.IsError)
                return requests.Errors;
            var templateRequests = requests.Value.Where(r => r.TemplateId == templateId).ToList();
            return templateRequests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching requests for template: {TemplateId}", templateId);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Request>> CreateAsync(Request request)
    {
        try
        {
            if (request == null)
                return ApplicationError.InvalidInput;

            if (request.TemplateId == Guid.Empty || request.RequesterId == Guid.Empty || request.ApproverId == Guid.Empty)
                return ApplicationError.InvalidInput;

            _logger.LogInformation("Creating new request for requester: {RequesterId}", request.RequesterId);

            await _unitOfWork.Requests.AddAsync(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created request: {RequestId}", request.Id);
            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating request");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Request>> UpdateAsync(Guid id, Request request)
    {
        try
        {
            if (id == Guid.Empty || request == null)
                return ApplicationError.InvalidInput;

            var existingRequest = await _unitOfWork.Requests.GetByIdAsync(id);
            if (existingRequest.IsError)
                return existingRequest.Errors;

            if (existingRequest.Value == null)
                return ApplicationError.RequestNotFound;

            _logger.LogInformation("Updating request: {RequestId}", id);

            existingRequest.Value.TemplateId = request.TemplateId;
            existingRequest.Value.RequesterId = request.RequesterId;
            existingRequest.Value.ApproverId = request.ApproverId;

            await _unitOfWork.Requests.UpdateAsync(existingRequest.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated request: {RequestId}", id);
            return existingRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating request: {RequestId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationError.InvalidInput;

            var request = await _unitOfWork.Requests.GetByIdAsync(id);
            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationError.RequestNotFound;

            _logger.LogInformation("Deleting request: {RequestId}", id);

            await _unitOfWork.Requests.RemoveAsync(x => x.Id == request.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted request: {RequestId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting request: {RequestId}", id);
            return ApplicationError.InternalServerError;
        }
    }
}
