using MagMini.Application.Common.Interfaces;
using MagMini.Infrastructure.Persistence;
using MagMini.Infrastructure.Persistence.Interceptors;
using MagMini.Infrastructure.Security;
using MagMini.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MagMini.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MagMiniDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddHttpClient<GusBirLookupService>();
        services.AddHttpClient<ICompanyLookupService, HybridCompanyLookupService>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<DbInitializer>();

        return services;
    }
}