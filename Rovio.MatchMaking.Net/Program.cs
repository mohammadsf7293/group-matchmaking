using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking;
using Rovio.MatchMaking.Net;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var connStr =  builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connStr,
    new MySqlServerVersion(new Version(8, 0, 21))));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(b => { b.RegisterModule<MatchMakingModule>(); });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IQueuedPlayerRepository, QueuedPlayerRepository>();


builder.Services.AddHttpContextAccessor();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(connStr);
logger.LogInformation("Rovio.MatchMaking.Net has booted up at {Time}", DateTime.UtcNow);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    logger.LogInformation("Started in development mode");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    const int maxRetries = 10;
    int retries = 0;
    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries++;
            logger.LogWarning(ex, "Migration failed. Retrying in 5 seconds... Attempt {Retry}/{MaxRetries}", retries, maxRetries);

            if (retries >= maxRetries)
            {
                logger.LogError("Max retries reached. Exiting.");
                throw;
            }

            Thread.Sleep(5000); // Wait 5 seconds
        }
    }
}


app.MapControllers();
var port = Environment.GetEnvironmentVariable("PORT") ?? "5003";
var host = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
app.Run($"http://{host}:{port}");