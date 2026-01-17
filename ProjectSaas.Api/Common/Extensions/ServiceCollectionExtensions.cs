using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectSaas.Api.Infrastructure.Data;
using ProjectSaas.Api.Application.Interfaces;
using ProjectSaas.Api.Infrastructure.Auth;
using ProjectSaas.Api.Application.Services;
using ProjectSaas.Api.Common.Middleware;

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

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ExceptionHandlingMiddleware>();

        return services;
    }

}
