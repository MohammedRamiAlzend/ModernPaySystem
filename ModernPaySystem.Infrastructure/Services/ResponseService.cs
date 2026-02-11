using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using Microsoft.AspNetCore.Http;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Response service CRUD operations.
/// </summary>
public class ResponseService(
    IUnitOfWork unitOfWork,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ResponseService> logger) : IResponseService
{
    public async Task<Result<IEnumerable<ResponseDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching responses for current user");

            // Get the current user ID from the HTTP context
            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            // Create an expression builder for Response entities
            var responseBuilder = new ExpressionBuilder<Response>();

            // Add a condition to filter responses where the RespondedByUserId matches the current user ID
            // This represents responses that were created BY the current user
            responseBuilder.And(r => r.RespondedByUserId == currentUserId);

            // Build the expression
            var expression = responseBuilder.Build();

            // Get responses that match the expression
            var responses = await unitOfWork.Responses.FindAsync(expression);
            if (responses.IsError)
                return responses.Errors;

            var responseDtos = responses.Value!.Select(r => r.ToDto()).ToList();

            // Return an empty list if no responses are found for the current user
            if (!responseDtos.Any())
                return new List<ResponseDto>();

            return responseDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching responses for current user");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged responses for current user, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            // Get the current user ID from the HTTP context
            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            // Create an expression builder for Response entities
            var responseBuilder = new ExpressionBuilder<Response>();

            // Add a condition to filter responses where the RespondedByUserId matches the current user ID
            // This represents responses that were created BY the current user
            responseBuilder.And(r => r.RespondedByUserId == currentUserId);

            // Build the expression
            var expression = responseBuilder.Build();

            // Get responses that match the expression with pagination
            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(page, pageSize, expression);
            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            var responseDtos = pagedResponses.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedResponseDtos = new PagedList<ResponseDto>(responseDtos, pagedResponses.Value.TotalItems, page, pageSize);

            return pagedResponseDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged responses for current user, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching response by id: {ResponseId}", id);
            var response = await unitOfWork.Responses.GetByIdAsync(id);

            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationErrors.ResponseNotFound;

            return response.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching response by id: {ResponseId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<ResponseDto>>> GetByRequestIdAsync(Guid requestId)
    {
        try
        {
            logger.LogInformation("Fetching responses for request: {requestId}", requestId);
            var responses = await unitOfWork.Responses.GetAllAsync();
            if (responses.IsError)
                return responses.Errors;

            var requestResponses = responses.Value!.Where(r => r.RequestId == requestId).Select(r => r.ToDto()).ToList();
            return requestResponses;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching responses for request: {requestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<ResponseDto>>> GetByResponderIdAsync(Guid responderId)
    {
        try
        {
            logger.LogInformation("Fetching responses for responder: {ResponderId}", responderId);
            var responses = await unitOfWork.Responses.GetAllAsync();
            if (responses.IsError)
                return responses.Errors;

            var responderResponses = responses.Value!.Where(r => r.RespondedByUserId == responderId).Select(r => r.ToDto()).ToList();
            return responderResponses;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching responses for responder: {ResponderId}", responderId);
            return ApplicationErrors.InternalServerError;
        }
    }
    public async Task<Result<bool>> IsRequestHasResponse(Guid requestId)
    {
        var checkIfRequestHasResponse = await unitOfWork.Requests.GetAsync(x => x.Id == requestId, i => i.Include(x => x.Response));
        if (checkIfRequestHasResponse.IsError)
        {
            return checkIfRequestHasResponse.Errors;
        }

        return checkIfRequestHasResponse.Value.Response is not null;
    }

    public async Task<Result<ResponseDto>> CreateAsync(CreateResponseDto response)
    {
        try
        {
            var isRequestHasResponseResult = await IsRequestHasResponse(response.RequestId);
            if (isRequestHasResponseResult.IsError)
                return isRequestHasResponseResult.Errors;

            if (response == null)
                return ApplicationErrors.InvalidInput;

            if (response.RequestId == Guid.Empty || response.RespondedByUserId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Creating new response for request: {requestId}", response.RequestId);

            var responseEntity = new Response
            {
                RequestId = response.RequestId,
                RespondedByUserId = response.RespondedByUserId,
                Comment = response.Comment
            };

            var addResult = await unitOfWork.Responses.AddAsync(responseEntity);
            if (addResult.IsError)
                return addResult.Errors;

            if (response.Files?.Any() == true)
            {
                logger.LogInformation("Uploading {FileCount} Files for new response: {ResponseId}", response.Files.Count, responseEntity.Id);
                foreach (var file in response.Files)
                {
                    var addFileToResponseResult = await webAttachmentService.UploadFileToResponseAsync(file, responseEntity.Id);
                    if (addFileToResponseResult.IsError)
                    {
                        return addFileToResponseResult.Errors;
                    }
                }
            }

            var getRequest = await unitOfWork.Requests.GetAsync(x => x.Id == response.RequestId);
            if(getRequest.IsError)
            {
                return getRequest.Errors;
            }

            getRequest.Value.ResponseId = responseEntity.Id;
            var updateResult = await unitOfWork.Requests.UpdateAsync(getRequest.Value);
            if(updateResult.IsError)
            {
                return updateResult.Errors;
            }

            logger.LogInformation("Successfully created response: {ResponseId}", responseEntity.Id);
            return responseEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating response");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseDto>> UpdateAsync(Guid id, UpdateResponseDto response)
    {
        try
        {
            if (id == Guid.Empty || response == null)
                return ApplicationErrors.InvalidInput;

            var existingResponse = await unitOfWork.Responses.GetByIdAsync(id);
            if (existingResponse.IsError)
                return existingResponse.Errors;

            if (existingResponse.Value == null)
                return ApplicationErrors.ResponseNotFound;

            logger.LogInformation("Updating response: {ResponseId}", id);

            existingResponse.Value.RequestId = response.RequestId;
            existingResponse.Value.RespondedByUserId = response.RespondedByUserId;
            existingResponse.Value.Comment = response.Comment;

            await unitOfWork.Responses.UpdateAsync(existingResponse.Value);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully updated response: {ResponseId}", id);
            return existingResponse.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating response: {ResponseId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var response = await unitOfWork.Responses.GetByIdAsync(id);
            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationErrors.ResponseNotFound;

            logger.LogInformation("Deleting response: {ResponseId}", id);

            await unitOfWork.Responses.RemoveAsync(x => x.Id == response.Value.Id);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully deleted response: {ResponseId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting response: {ResponseId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseDto>> AddFilesToResponseAsync(Guid responseId, List<IFormFile> files)
    {
        try
        {
            if (responseId == Guid.Empty || files == null || !files.Any())
                return ApplicationErrors.InvalidInput;

            // Verify the response exists
            var response = await unitOfWork.Responses.GetByIdAsync(responseId);
            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationErrors.ResponseNotFound;

            logger.LogInformation("Adding {FileCount} Files to response: {ResponseId}", files.Count, responseId);

            // Process each file and associate it with the response using WebAttachmentService
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var uploadResult = await webAttachmentService.UploadFileToResponseAsync(file, response.Value.Id);
                    if (uploadResult.IsError)
                        return uploadResult.Errors;
                }
            }

            logger.LogInformation("Successfully added {FileCount} Files to response: {ResponseId}", files.Count, responseId);

            // Return the updated response with its attachments
            var updatedResponse = await unitOfWork.Responses.GetByIdAsync(responseId);
            if (updatedResponse.IsError)
                return updatedResponse.Errors;

            return updatedResponse.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding Files to response: {ResponseId}", responseId);
            return ApplicationErrors.InternalServerError;
        }
    }
}
