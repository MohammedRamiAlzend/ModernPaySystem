using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of User service CRUD operations.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all users");
            var users = await _unitOfWork.Users.GetAllAsync();
            if(users.IsError)
                return users.Errors;

            var userDtos = users.Value!.Select(u => u.ToDto()).ToList();
            return userDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<UserDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged users, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedUsers = await _unitOfWork.Users.GetPagedAsync(page, pageSize);
            if (pagedUsers.IsError)
                return pagedUsers.Errors;

            var userDtos = pagedUsers.Value!.Items.Select(u => u.ToDto()).ToList();
            var pagedUserDtos = new PagedList<UserDto>(userDtos, pagedUsers.Value.TotalItems, page, pageSize);

            return pagedUserDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged users, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching user by id: {UserId}", id);
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationErrors.UserNotFound;

            return user.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by id: {UserId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> GetByUsernameAsync(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return ApplicationErrors.InvalidInput;

            _logger.LogInformation("Fetching user by username: {Username}", username);

            var users = await _unitOfWork.Users.GetAllAsync();
            if(users.IsError)
            {
                return users.Errors;
            }

            var user = users.Value!.Find(u => u.UserName == username);

            return user == null ? (Result<UserDto>)ApplicationErrors.UserNotFound : (Result<UserDto>)user.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by username: {Username}", username);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _unitOfWork.Users.AnyAsync(u => u.UserName == username);
        }
        catch
        {
            return false;
        }
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto user)
    {
        try
        {
            if (user == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(user.UserName))
                return ApplicationErrors.MissingRequiredField;

            if (await UsernameExistsAsync(user.UserName))
                return ApplicationErrors.UserAlreadyExists;

            _logger.LogInformation("Creating new user: {Username}", user.UserName);

            var userEntity = new User
            {
                UserName = user.UserName,
                HashedPassword = user.HashedPassword,
                SubSystemUserId = user.SubSystemUserId
            };

            var addResult = await _unitOfWork.Users.AddAsync(userEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await _unitOfWork.SaveChangesAsync();
            if (result < 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully created user: {Username}", user.UserName);
            return userEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto user)
    {
        try
        {
            if (id == Guid.Empty || user == null)
                return ApplicationErrors.InvalidInput;

            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser.IsError)
                return existingUser.Errors;

            if (existingUser.Value == null)
                return ApplicationErrors.UserNotFound;

            _logger.LogInformation("Updating user: {UserId}", id);

            existingUser.Value.UserName = user.UserName;
            existingUser.Value.HashedPassword = user.HashedPassword;
            existingUser.Value.SubSystemUserId = user.SubSystemUserId;

            var updateResult = await _unitOfWork.Users.UpdateAsync(existingUser.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated user: {UserId}", id);
            return existingUser.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationErrors.UserNotFound;

            _logger.LogInformation("Deleting user: {UserId}", id);

            await _unitOfWork.Users.RemoveAsync(x => x.Id == user.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted user: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
}
