using Demo.Application.Catalog.Abstractions;
using Demo.Application.Catalog.Services;
using Demo.Application.Common.Behaviors;
using Demo.Application.Security.Abstractions;
using Demo.Application.Security.Commands;
using Demo.Application.Security.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IValidator<UserProfileUpdateCommand>, UserProfileUpdateValidator>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
