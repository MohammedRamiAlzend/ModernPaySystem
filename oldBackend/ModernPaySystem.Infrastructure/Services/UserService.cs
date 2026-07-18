using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of User service CRUD operations.
/// </summary>
public class UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ILogger<UserService> logger) : IUserService
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
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<UserDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged users, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

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
            logger.LogError(ex, "Error fetching paged users, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching user by id: {UserId}", id);
            var user = await unitOfWork.Users.GetAsync(
                filter: UserExpressions.ById(id),
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );

            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationErrors.OperationFailed;

            return user.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by id: {UserId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> GetByUsernameAsync(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Fetching user by username: {Username}", username);
            var user = await unitOfWork.Users.GetAsync(
                filter: UserExpressions.ByUsername(username),
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department)
            );

            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationErrors.OperationFailed;

            return user.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user by username: {Username}", username);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto user)
    {
        try
        {
            if (user == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
                return ApplicationErrors.MissingRequiredField;

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

            var subSystemUser = new SubSystemUser()
            {
                UserId = userEntity.Id,
                User = userEntity,
                SubSystem = user.SubSystem
            };

            var enrollUserToSubSystem = await unitOfWork.SubSystemUsers.AddAsync(subSystemUser);
            if (enrollUserToSubSystem.IsError)
                return enrollUserToSubSystem.Errors;

            if (user.DepartmentId.HasValue && user.IsArchiveLeader)
            {

                var re = await unitOfWork.DepartmentArchiveLeaders.AddAsync(new DepartmentArchiveLeader
                {
                    UserId = userEntity.Id,
                    DepartmentId = user.DepartmentId.Value
                });
                if (re.IsError) return re.Errors;
            }

            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully created user: {Username}", user.UserName);
            
            var getUser = await unitOfWork.Users.GetAsync(x => x.Id == userEntity.Id, i => i.Include(x => x.DepartmentArchiveLeaders).Include(x => x.Department).Include(x => x.HeadedDepartment));
            
            return getUser.IsError is false ? getUser.Value!.ToDto() : getUser.Errors;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, CreateUserDto userDto)
    {
        try
        {
            if (id == Guid.Empty || userDto == null)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Updating user: {UserId}", id);

            var userResult = await unitOfWork.Users.GetAsync(
                filter: x => x.Id == id,
                transform: query => query.Include(x => x.SubSystemUser)
            );

            if (userResult.IsError)
                return userResult.Errors;

            if (userResult.Value == null)
                return ApplicationErrors.OperationFailed;

            var userEntity = userResult.Value;

            // Update basic info
            userEntity.UserName = userDto.UserName;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                userEntity.HashedPassword = passwordHasher.HashPassword(userDto.Password);
            }

            // Update SubSystem
            if (userEntity.SubSystemUser != null)
            {
                userEntity.SubSystemUser.SubSystem = userDto.SubSystem ?? SubSystem.None;
            }
            else
            {
                var subSystemUser = new SubSystemUser()
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
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var user = await unitOfWork.Users.GetByIdAsync(id);
            if (user.IsError)
                return user.Errors;

            if (user.Value == null)
                return ApplicationErrors.OperationFailed;

            logger.LogInformation("Deleting user: {UserId}", id);

            await unitOfWork.Users.RemoveAsync(x => x.Id == user.Value.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully deleted user: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user: {UserId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<UserDto>>> GetBySubSystemAsync(SubSystem subSystem)
    {
        try
        {
            logger.LogInformation("Fetching users by subsystem: {SubSystem}", subSystem);

            var users = await unitOfWork.Users.GetAllAsync(
                transform: query => query.Include(x => x.SubSystemUser).Include(x => x.Department),
                additionalFilters: UserExpressions.BySubSystemWithIncludes(subSystem)
            );
            if (users.IsError)
                return users.Errors;

            var userDtos = users.Value!.ConvertAll(u => u.ToDto());
            return userDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching users by subsystem: {SubSystem}", subSystem);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetVisitedTemplatesAsync(Guid userId)
    {
        try
        {
            logger.LogInformation("Fetching visited templates for user: {UserId}", userId);
            var user = await unitOfWork.Users.GetAsync(filter: u => u.Id == userId, transform: q => q.Include(u => u.VisitedTemplates));
            if (user.IsError)
                return user.Errors;
            if (user.Value == null)
                return ApplicationErrors.OperationFailed;
            var templates = user.Value.VisitedTemplates.Select(t => t.ToDto()).ToList();
            return templates;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching visited templates for user: {UserId}", userId);
            return ApplicationErrors.InternalServerError;
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
                return ApplicationErrors.UserNotFound;

            if (!currentUser.Value.DepartmentId.HasValue)
                return ApplicationErrors.DepartmentNotFound;

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
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<SubSystemDto>>> GetSubSystemsAsync()
    {
        try
        {
            return (List<SubSystemDto>)[

                new SubSystemDto
                {
                    Name = nameof(SubSystem.TransactionSystem),
                    Value = ((int)SubSystem.TransactionSystem).ToString()
                },
                new SubSystemDto
                {
                    Name = nameof(SubSystem.None),
                    Value = ((int)SubSystem.None).ToString()
                },
                new SubSystemDto
                {
                    Name = nameof(SubSystem.Diwan),
                    Value = ((int)SubSystem.Diwan).ToString()
                },
new SubSystemDto
                {
                    Name = nameof(SubSystem.Archiving),
                    Value = ((int)SubSystem.Archiving).ToString()
                },
new SubSystemDto
                {
                    Name = nameof(SubSystem.Shared),
                    Value = ((int)SubSystem.Shared).ToString()
                },


            ];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subsystems for users");
            return ApplicationErrors.InternalServerError;
        }
    }
}
