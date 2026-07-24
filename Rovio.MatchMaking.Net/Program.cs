using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking;
using Rovio.MatchMaking.Application;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. Set ConnectionStrings__DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IQueuedPlayerRepository, QueuedPlayerRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<MatchMakingService>();
builder.Services.AddScoped<SessionMatchMaker>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
