using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Infrastructure.Queries;
using Identity.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Repositories;

namespace Identity.Infrastructure.Services
{
    public static class ServicesRegister
    {
        public static void AddIdentityServices(this IServiceCollection services)
        {
            // Register unit of work
            // Note: Repositories are created by UnitOfWork directly (not through DI)
            // This ensures all repositories use the same DbContext instance as UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IUserQueries, UserQueries>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();

            // Register Dapper query services (read-model)
            services.AddScoped<IDapperExecutor, DapperExecutor>();
        }
    }
}
