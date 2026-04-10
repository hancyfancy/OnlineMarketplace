using Demo.Application;
using Demo.Application.Security.Abstractions;
using Demo.Composition.Services;
using Demo.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Composition;

public static class DependencyInjection
{
    public static IServiceCollection AddComposition(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
