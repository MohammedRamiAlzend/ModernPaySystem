using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;

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

    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all users");
            var users = await _unitOfWork.Users.GetAllAsync();
            if(users.IsError)
                return users.Errors;

            return users.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<User>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching user by id: {UserId}", id);
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationError.UserNotFound;

            return user!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by id: {UserId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<User>> GetByUsernameAsync(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return ApplicationError.InvalidInput;

            _logger.LogInformation("Fetching user by username: {Username}", username);

            var users = await _unitOfWork.Users.GetAllAsync();
            if(users.IsError)
            {
                return users.Errors;
            }

            var user = users.Value!.Find(u => u.UserName == username);

            return user == null ? (Result<User>)ApplicationError.UserNotFound : (Result<User>)user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user by username: {Username}", username);
            return ApplicationError.InternalServerError;
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

    public async Task<Result<User>> CreateAsync(User user)
    {
        try
        {
            if (user == null)
                return ApplicationError.InvalidInput;

            if (string.IsNullOrWhiteSpace(user.UserName))
                return ApplicationError.MissingRequiredField;

            if (await UsernameExistsAsync(user.UserName))
                return ApplicationError.UserAlreadyExists;

            _logger.LogInformation("Creating new user: {Username}", user.UserName);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created user: {Username}", user.UserName);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<User>> UpdateAsync(Guid id, User user)
    {
        try
        {
            if (id == Guid.Empty || user == null)
                return ApplicationError.InvalidInput;

            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser.IsError)
                return existingUser.Errors;

            if (existingUser.Value == null)
                return ApplicationError.UserNotFound;

            _logger.LogInformation("Updating user: {UserId}", id);

            existingUser.Value.UserName = user.UserName;
            existingUser.Value.HashedPassword = user.HashedPassword;

            await _unitOfWork.Users.UpdateAsync(existingUser.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated user: {UserId}", id);
            return existingUser!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationError.InvalidInput;

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationError.UserNotFound;

            _logger.LogInformation("Deleting user: {UserId}", id);

            await _unitOfWork.Users.RemoveAsync(x => x.Id == user.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted user: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            return ApplicationError.InternalServerError;
        }
    }
}
