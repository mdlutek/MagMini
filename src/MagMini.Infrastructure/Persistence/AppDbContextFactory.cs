using MagMini.Application.Common.Interfaces;
using MagMini.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MagMini.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=MagMiniDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        optionsBuilder.UseSqlServer(connectionString);

        // Atrapka ICurrentUserService tylko na czas tworzenia migracji
        var dummyCurrentUserService = new DesignTimeCurrentUserService();
        var interceptor = new AuditableEntityInterceptor(dummyCurrentUserService);

        return new AppDbContext(optionsBuilder.Options, interceptor);
    }
}

internal class DesignTimeCurrentUserService : ICurrentUserService
{
    public int? UserId => null;
    public string? Username => "MIGRATION_DESIGN_TIME";
    public bool IsAuthenticated => false;
}