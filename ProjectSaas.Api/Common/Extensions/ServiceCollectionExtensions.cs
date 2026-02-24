using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectSaas.Api.Infrastructure.Data;
using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Infrastructure.Auth;
using ProjectSaas.Api.Application.Services;
using ProjectSaas.Api.Common.Middleware;
using ProjectSaas.Api.Application.Abstractions.Security;
using ProjectSaas.Api.Application.Abstractions.Auth;
using ProjectSaas.Api.Application.Abstractions.Tenancy;
using ProjectSaas.Api.Infrastructure.Tenancy;
using ProjectSaas.Api.Application.Tickets;

namespace ProjectSaas.Api.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        //automatically called as a scoped service
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<TenantContextAccessor>();
        services.AddScoped<ProjectSaas.Api.Application.Abstractions.Tenancy.ITenantContext>(
            sp => sp.GetRequiredService<TenantContextAccessor>());

        services.AddScoped<TenantContextMiddleware>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITicketService, TicketService>();

        return services;
    }

}
