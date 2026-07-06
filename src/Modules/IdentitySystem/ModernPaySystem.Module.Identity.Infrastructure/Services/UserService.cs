using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Identity.Application;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.DTOs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Services;

public class UserService(
    IIdentityUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all users");
            var users = await unitOfWork.Users.GetAllAsync(
                transform: query => query.Include(x => x.SubSystemUser)
                                         .Include(x => x.Department)
            );
            if (users.IsError)
                return users.Errors;

            var userDtos = users.Value!.ConvertAll(u => u.ToDto());
            return userDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all users");
            return Error.Failure("InternalServerError", "An error occurred while fetching users.");
        }
    }

    public async Task<Result<PagedList<UserDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged users, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return new Error("InvalidInput", "Page must be greater than 0.", ErrorKind.Validation);
            if (pageSize <= 0 || pageSize > 100)
                return new Error("InvalidInput", "Page size must be between 1 and 100.", ErrorKind.Validation);

            var pagedUsers = await unitOfWork.Users.GetPagedAsync(
                page,
                pageSize,
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );
            if (pagedUsers.IsError)
                return pagedUsers.Errors;

            var userDtos = pagedUsers.Value!.Items.Select(u => u.ToDto()).ToList();
            var pagedUserDtos = new PagedList<UserDto>(userDtos, pagedUsers.Value.TotalItems, page, pageSize);

            return pagedUserDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged users");
            return Error.Failure("InternalServerError", "An error occurred while fetching users.");
        }
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching user by id: {UserId}", id);
            var user = await unitOfWork.Users.GetAsync(
                filter: x => x.Id == id,
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );

            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return new Error("NotFound", "User not found.", ErrorKind.NotFound);

            return user.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by id: {UserId}", id);
            return Error.Failure("InternalServerError", "An error occurred while fetching the user.");
        }
    }

    public async Task<Result<UserDto>> GetByUsernameAsync(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return new Error("InvalidInput", "Username is required.", ErrorKind.Validation);

            logger.LogInformation("Fetching user by username: {Username}", username);
            var user = await unitOfWork.Users.GetAsync(
                filter: x => x.UserName == username,
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );

            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return new Error("NotFound", "User not found.", ErrorKind.NotFound);

            return user.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by username: {Username}", username);
            return Error.Failure("InternalServerError", "An error occurred while fetching the user.");
        }
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto user)
    {
        try
        {
            if (user == null)
                return new Error("InvalidInput", "User data is required.", ErrorKind.Validation);

            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
                return new Error("MissingRequiredField", "Username and password are required.", ErrorKind.Validation);

            logger.LogInformation("Creating new user: {Username}", user.UserName);

            var userEntity = new User
            {
                UserName = user.UserName,
                HashedPassword = passwordHasher.HashPassword(user.Password),
                DepartmentId = user.DepartmentId,
                IsDepartmentHead = user.IsDepartmentHead,
                HeadedDepartmentId = user.DepartmentId
            };

            var addResult = await unitOfWork.Users.AddAsync(userEntity);
            if (addResult.IsError)
                return addResult.Errors;

            var subSystemUser = new SubSystemUser
            {
                UserId = userEntity.Id,
                User = userEntity,
                SubSystem = user.SubSystem
            };

            var enrollResult = await unitOfWork.SubSystemUsers.AddAsync(subSystemUser);
            if (enrollResult.IsError)
                return enrollResult.Errors;

            int saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return new Error("DatabaseError", "Failed to save user to database.", ErrorKind.Failure);

            logger.LogInformation("Successfully created user: {Username}", user.UserName);

            var getUser = await unitOfWork.Users.GetAsync(
                x => x.Id == userEntity.Id,
                i => i.Include(x => x.Department).Include(x => x.HeadedDepartment));

            return getUser.IsError ? getUser.Errors : getUser.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return Error.Failure("InternalServerError", "An error occurred while creating the user.");
        }
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, CreateUserDto userDto)
    {
        try
        {
            if (id == Guid.Empty || userDto == null)
                return new Error("InvalidInput", "User ID and data are required.", ErrorKind.Validation);

            logger.LogInformation("Updating user: {UserId}", id);

            var userResult = await unitOfWork.Users.GetAsync(
                filter: x => x.Id == id,
                transform: query => query.Include(x => x.SubSystemUser)
            );

            if (userResult.IsError)
                return userResult.Errors;

            if (userResult.Value == null)
                return new Error("NotFound", "User not found.", ErrorKind.NotFound);

            var userEntity = userResult.Value;

            userEntity.UserName = userDto.UserName;

            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                userEntity.HashedPassword = passwordHasher.HashPassword(userDto.Password);
            }

            if (userEntity.SubSystemUser != null)
            {
                userEntity.SubSystemUser.SubSystem = userDto.SubSystem ?? SubSystem.None;
            }
            else
            {
                var subSystemUser = new SubSystemUser
                {
                    UserId = userEntity.Id,
                    SubSystem = userDto.SubSystem ?? SubSystem.None
                };
                await unitOfWork.SubSystemUsers.AddAsync(subSystemUser);
            }

            int result = await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully updated user: {UserId}", id);
            return userEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user: {UserId}", id);
            return Error.Failure("InternalServerError", "An error occurred while updating the user.");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new Error("InvalidInput", "User ID is required.", ErrorKind.Validation);

            var user = await unitOfWork.Users.GetByIdAsync(id);
            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return new Error("NotFound", "User not found.", ErrorKind.NotFound);

            logger.LogInformation("Deleting user: {UserId}", id);

            await unitOfWork.Users.RemoveAsync(x => x.Id == user.Value.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return new Error("DatabaseError", "Failed to delete user.", ErrorKind.Failure);

            logger.LogInformation("Successfully deleted user: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user: {UserId}", id);
            return Error.Failure("InternalServerError", "An error occurred while deleting the user.");
        }
    }

    public async Task<Result<IEnumerable<UserDto>>> GetBySubSystemAsync(SubSystem subSystem)
    {
        try
        {
            logger.LogInformation("Fetching users by subsystem: {SubSystem}", subSystem);

            var users = await unitOfWork.Users.GetAllAsync(
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );

            if (users.IsError)
                return users.Errors;

            var filtered = users.Value!
                .Where(u => u.SubSystemUser != null && u.SubSystemUser.SubSystem == subSystem)
                .Select(u => u.ToDto())
                .ToList();

            return filtered;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching users by subsystem: {SubSystem}", subSystem);
            return Error.Failure("InternalServerError", "An error occurred while fetching users.");
        }
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetVisitedTemplatesAsync(Guid userId)
    {
        try
        {
            logger.LogInformation("Fetching visited templates for user: {UserId}", userId);
            var user = await unitOfWork.Users.GetAsync(
                filter: u => u.Id == userId,
                transform: q => q.Include(u => u.Roles));

            if (user.IsError)
                return user.Errors;
            if (user.Value == null)
                return new Error("NotFound", "User not found.", ErrorKind.NotFound);

            return new List<TemplateDto>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching visited templates for user: {UserId}", userId);
            return Error.Failure("InternalServerError", "An error occurred while fetching visited templates.");
        }
    }

    public async Task<Result<IEnumerable<UserDto>>> GetCurrentDepartmentUsersAsync(Guid currentUserId)
    {
        try
        {
            logger.LogInformation("Fetching users in same department as user {UserId}", currentUserId);

            var currentUser = await unitOfWork.Users.GetAsync(
                filter: x => x.Id == currentUserId,
                transform: query => query.Include(x => x.Department)
            );

            if (currentUser.IsError || currentUser.Value == null)
                return new Error("UserNotFound", "User not found.", ErrorKind.NotFound);

            if (!currentUser.Value.DepartmentId.HasValue)
                return new Error("DepartmentNotFound", "User has no department.", ErrorKind.NotFound);

            var departmentId = currentUser.Value.DepartmentId.Value;

            var users = await unitOfWork.Users.GetAllAsync(
                filter: x => x.DepartmentId == departmentId,
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );

            if (users.IsError)
                return users.Errors;

            var userDtos = users.Value!.ConvertAll(u => u.ToDto());
            logger.LogInformation("Found {Count} users in department {DepartmentId}", userDtos.Count, departmentId);
            return userDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching users in department for user {UserId}", currentUserId);
            return Error.Failure("InternalServerError", "An error occurred while fetching users.");
        }
    }

    public async Task<Result<List<SubSystemDto>>> GetSubSystemsAsync()
    {
        try
        {
            return new List<SubSystemDto>
            {
                new SubSystemDto { Name = nameof(SubSystem.TransactionSystem), Value = ((int)SubSystem.TransactionSystem).ToString() },
                new SubSystemDto { Name = nameof(SubSystem.None), Value = ((int)SubSystem.None).ToString() },
                new SubSystemDto { Name = nameof(SubSystem.Diwan), Value = ((int)SubSystem.Diwan).ToString() },
                new SubSystemDto { Name = nameof(SubSystem.Archiving), Value = ((int)SubSystem.Archiving).ToString() },
                new SubSystemDto { Name = nameof(SubSystem.Shared), Value = ((int)SubSystem.Shared).ToString() },
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subsystems");
            return Error.Failure("InternalServerError", "An error occurred while fetching subsystems.");
        }
    }
}
