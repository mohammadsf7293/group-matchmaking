using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rovio.MatchMaking.Console.Services;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;

namespace Rovio.MatchMaking.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").AddEnvironmentVariables()
                .Build();

            // Create a Host to manage dependency injection and configuration
            var host = CreateHostBuilder(args, configuration).Build(); // Pass configuration as parameter

            // Use the host's service provider to get the SessionMatchMaker service and run it
            var matchmaker = host.Services.GetRequiredService<SessionMatchMaker>();

            try
            {
                // Execute the matchmaking process
                await matchmaker.RunAsync();
            }
            catch (Exception ex)
            {
                // Handle any unhandled exceptions
                System.Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Environment.Exit(1); // Exit with error code 1
            }

            System.Console.WriteLine("Matchmaking process completed successfully.");
        }

        // Configure the host builder and services
        static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configure DbContext with connection string
                    var connectionString =  configuration.GetConnectionString("DefaultConnection");
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

                    // Register repositories and services
                    services.AddScoped<ISessionRepository, SessionRepository>();
                    services.AddScoped<IQueuedPlayerRepository, QueuedPlayerRepository>();
                    services.AddScoped<SessionMatchMaker>();

                    // Add additional services or configurations if needed
                });
    }
}
