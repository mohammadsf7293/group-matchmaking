using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rovio.MatchMaking;
using Rovio.MatchMaking.Application;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;

namespace Rovio.MatchMaking.Console;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException(
                        "Connection string 'DefaultConnection' is missing. Set ConnectionStrings__DefaultConnection.");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

                services.AddScoped<ISessionRepository, SessionRepository>();
                services.AddScoped<IQueuedPlayerRepository, QueuedPlayerRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<SessionMatchMaker>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

        try
        {
            using var scope = host.Services.CreateScope();
            var matchmaker = scope.ServiceProvider.GetRequiredService<SessionMatchMaker>();
            await matchmaker.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Matchmaking process failed");
            return 1;
        }
    }
}
