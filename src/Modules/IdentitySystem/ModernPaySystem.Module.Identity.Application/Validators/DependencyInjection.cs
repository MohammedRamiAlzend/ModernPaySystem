using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ModernPaySystem.Module.Identity.Application.Validators;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
        return services;
    }
}
