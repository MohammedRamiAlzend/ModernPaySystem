using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace ModernPaySystem.Infrastructure.Services;

public class ResponseService(
    IUnitOfWork unitOfWork,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ResponseService> logger) : IResponseService
{
    public async Task<Result<PagedList<ResponseDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged responses for current user, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(
                page,
                pageSize,
                transform: x => x
                .Include(x => x.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                .Include(x => x.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: [ResponseExpressions.ByRespondedByUserId(currentUserId)]);

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

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var response = await unitOfWork.Responses.GetAsync(
                filter: r => r.Id == id,
                transform: x => x
                .Include(x => x.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                .Include(x => x.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                .Include(x => x.ResponseAttachments),
                additionalFilters: [ResponseExpressions.CanReadByUserId(currentUserId)]);

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

    public async Task<Result<PagedList<ResponseDto>>> GetByRequestIdAsync(Guid requestId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged responses for request: {RequestId}, page: {Page}, size: {PageSize}", requestId, page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(
                page,
                pageSize,
                transform: i =>
                i.Include(r => r.ResponseAttachments)
                .Include(r => r.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                .Include(r => r.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: ResponseExpressions.ByRequestIdWithIncludes(requestId));

            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            var responseDtos = pagedResponses.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<ResponseDto>(responseDtos, pagedResponses.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged responses for request: {RequestId}, page: {Page}, size: {PageSize}", requestId, page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ResponseDto>>> GetByResponderIdAsync(Guid responderId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged responses for responder: {ResponderId}, page: {Page}, size: {PageSize}", responderId, page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(r => r.ResponseAttachments)
                .Include(r => r.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                .Include(r => r.Request).ThenInclude(r => r!.RequestAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues),
                additionalFilters: ResponseExpressions.ByRespondedByUserIdWithIncludes(responderId));

            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            var responseDtos = pagedResponses.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<ResponseDto>(responseDtos, pagedResponses.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged responses for responder: {ResponderId}, page: {Page}, size: {PageSize}", responderId, page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }
    public async Task<Result<PagedList<ResponseDto>>> GetResponsesByRequesterIdAsync(Guid requesterId, int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged responses for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, page, pageSize);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var pagedResponses = await unitOfWork.Responses.GetPagedAsync(
                page,
                pageSize,
                transform: i => i.Include(r => r.Request).ThenInclude(r => r!.RequestAttachments)
                .Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                .Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                ,
                additionalFilters: ResponseExpressions.ByRequesterIdWithIncludes(requesterId));

            if (pagedResponses.IsError)
                return pagedResponses.Errors;

            var responseDtos = pagedResponses.Value!.Items.Select(r => r.ToDto()).ToList();
            return new PagedList<ResponseDto>(responseDtos, pagedResponses.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged responses for requester: {RequesterId}, page: {Page}, size: {PageSize}", requesterId, page, pageSize);
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

        return checkIfRequestHasResponse.Value!.Response is not null;
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

            getRequest.Value!.ResponseId = responseEntity.Id;
            getRequest.Value.Status = RequestStatus.Managed;
            var updateResult = await unitOfWork.Requests.UpdateAsync(getRequest.Value);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }
            logger.LogInformation("Successfully created response: {ResponseId}", responseEntity.Id);
            await unitOfWork.SaveChangesAsync();
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

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var existingResponse = await unitOfWork.Responses.GetAsync(
                filter: r => r.Id == id,
                transform: x => x
                .Include(x => x.ResponseAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.Template)
                .Include(x => x.ResponseAttachments).Include(x => x.Request).ThenInclude(r => r.RequestTemplateValues).ThenInclude(x => x!.InputValues)
                , additionalFilters: [ResponseExpressions.CanMakeUpdateByUserId(currentUserId)]);

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

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var response = await unitOfWork.Responses.GetAsync(
                filter: r => r.Id == id,
                additionalFilters: new List<Expression<Func<Response, bool>>> { ResponseExpressions.CanMakeUpdateByUserId(currentUserId) });

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

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var response = await unitOfWork.Responses.GetAsync(
                filter: r => r.Id == responseId,
                transform: x => x.Include(x => x.ResponseAttachments),
                additionalFilters: new List<Expression<Func<Response, bool>>> { ResponseExpressions.CanMakeUpdateByUserId(currentUserId) });

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

            var updatedResponse = await unitOfWork.Responses.GetAsync(
                filter: r => r.Id == responseId,
                transform: x => x.Include(x => x.Request)
                                 .Include(x => x.ResponseAttachments));

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
