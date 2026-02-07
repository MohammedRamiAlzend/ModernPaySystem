using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Response service CRUD operations.
/// </summary>
public class ResponseService : IResponseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResponseService> _logger;

    public ResponseService(IUnitOfWork unitOfWork, ILogger<ResponseService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Response>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all responses");
            var responses = await _unitOfWork.Responses.GetAllAsync();
            if (responses.IsError)
            {
                return responses.Errors;
            }

            return responses.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all responses");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<PagedList<Response>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged responses, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationError.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationError.InvalidInput;

            var pagedResponses = await _unitOfWork.Responses.GetPagedAsync(page, pageSize);
            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            return pagedResponses.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged responses, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Response>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching response by id: {ResponseId}", id);
            var response = await _unitOfWork.Responses.GetByIdAsync(id);

            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationError.ResponseNotFound;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching response by id: {ResponseId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Response>>> GetByRequestIdAsync(Guid requestId)
    {
        try
        {
            _logger.LogInformation("Fetching responses for request: {RequestId}", requestId);
            var responses = await _unitOfWork.Responses.GetAllAsync();
            if (responses.IsError)
                return responses.Errors;

            var requestResponses = responses.Value.Where(r => r.RequestId == requestId).ToList();
            return requestResponses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching responses for request: {RequestId}", requestId);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<Response>>> GetByResponderIdAsync(Guid responderId)
    {
        try
        {
            _logger.LogInformation("Fetching responses for responder: {ResponderId}", responderId);
            var responses = await _unitOfWork.Responses.GetAllAsync();
            if (responses.IsError)
                return responses.Errors;

            var responderResponses = responses.Value.Where(r => r.RespondedByUserId == responderId).ToList();
            return responderResponses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching responses for responder: {ResponderId}", responderId);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Response>> CreateAsync(Response response)
    {
        try
        {
            if (response == null)
                return ApplicationError.InvalidInput;

            if (response.RequestId == Guid.Empty || response.RespondedByUserId == Guid.Empty)
                return ApplicationError.InvalidInput;

            _logger.LogInformation("Creating new response for request: {RequestId}", response.RequestId);

            await _unitOfWork.Responses.AddAsync(response);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created response: {ResponseId}", response.Id);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating response");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Response>> UpdateAsync(Guid id, Response response)
    {
        try
        {
            if (id == Guid.Empty || response == null)
                return ApplicationError.InvalidInput;

            var existingResponse = await _unitOfWork.Responses.GetByIdAsync(id);
            if (existingResponse.IsError)
                return existingResponse.Errors;

            if (existingResponse.Value == null)
                return ApplicationError.ResponseNotFound;

            _logger.LogInformation("Updating response: {ResponseId}", id);

            existingResponse.Value.RequestId = response.RequestId;
            existingResponse.Value.RespondedByUserId = response.RespondedByUserId;
            existingResponse.Value.Comment = response.Comment;

            await _unitOfWork.Responses.UpdateAsync(existingResponse.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated response: {ResponseId}", id);
            return existingResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating response: {ResponseId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationError.InvalidInput;

            var response = await _unitOfWork.Responses.GetByIdAsync(id);
            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationError.ResponseNotFound;

            _logger.LogInformation("Deleting response: {ResponseId}", id);

            await _unitOfWork.Responses.RemoveAsync(x => x.Id == response.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted response: {ResponseId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting response: {ResponseId}", id);
            return ApplicationError.InternalServerError;
        }
    }
}
