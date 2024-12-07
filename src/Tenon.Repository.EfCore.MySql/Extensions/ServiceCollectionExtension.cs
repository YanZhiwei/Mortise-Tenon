using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tenon.Repository.EfCore.Configurations;
using Tenon.Repository.EfCore.Extensions;

namespace Tenon.Repository.EfCore.MySql.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEfCoreMySql<TDbContext>(
        this IServiceCollection services,
        IConfigurationSection mysqlSection,
        Action<MySqlDbContextOptionsBuilder>? mysqlOptionsAction = null,
        IInterceptor[]? interceptors = null)
        where TDbContext : DbContext
    {
        return services.AddEfCore<TDbContext>(
            mysqlSection,
            options =>
            {
                var config = mysqlSection.Get<DbOptions>();
                options.UseMySql(
                    config.ConnectionString,
                    ServerVersion.AutoDetect(config.ConnectionString),
                    mysqlOptionsAction);
            },
            interceptors);
    }
}