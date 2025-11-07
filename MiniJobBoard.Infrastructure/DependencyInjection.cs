using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniJobBoard.Infrastructure.Data;

namespace MiniJobBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var useInMemory = config.GetValue<bool>("Persistence:UseInMemory");
        if (useInMemory)
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("MiniJobBoard"));
        }
        else
        {
            var conn = config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(conn));
        }
        return services;
    }
}
