using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using Microsoft.AspNetCore.Http;
using ExpressionBuilderLib.src.Core;
using System.Linq.Expressions;

namespace ModernPaySystem.Infrastructure.Services;

public class ResponseService(
    IUnitOfWork unitOfWork,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ResponseService> logger) : IResponseService
{
    public Expression<Func<Response, bool>> UserFilter()
    {
        var currentUserId = httpContextServiceManager.GetCurrentUserId();
        var responseBuilder = new ExpressionBuilder<Response>();
        responseBuilder.And(r => r.RespondedByUserId == currentUserId);
        return responseBuilder.Build();
    }
    public async Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged responses for current user, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(page, pageSize, UserFilter());
            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            var responseDtos = pagedResponses.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<ResponseDto>(responseDtos, pagedResponses.Value.TotalItems, page, pageSize);
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
            var responses = await unitOfWork.Responses.GetAllAsync(null, x => x.Include(i => i.ResponseAttachments));
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
    public async Task<Result<IEnumerable<ResponseDto>>> GetResponsesByRequesterIdAsync(Guid requesterId, int page = 1, int pageSize = 10)
    {
        try
        {
            logger.LogInformation("Fetching responses for requester: {requesterId}", requesterId);
            var responses = await unitOfWork.Responses.GetAllAsync(
                r => r.Request.RequesterId == requesterId,
                i => i.Include(x => x.Request));

            if (responses.IsError)
                return responses.Errors;

            var responderResponses = responses.Value!.ConvertAll(r => r.ToDto());
            return responderResponses;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching responses for responder: {ResponderId}", requesterId);
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
            if (getRequest.IsError)
            {
                return getRequest.Errors;
            }

            getRequest.Value.ResponseId = responseEntity.Id;
            var updateResult = await unitOfWork.Requests.UpdateAsync(getRequest.Value);
            if (updateResult.IsError)
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
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

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
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

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

            var response = await unitOfWork.Responses.GetByIdAsync(responseId);
            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationErrors.ResponseNotFound;

            logger.LogInformation("Adding {FileCount} Files to response: {ResponseId}", files.Count, responseId);

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
