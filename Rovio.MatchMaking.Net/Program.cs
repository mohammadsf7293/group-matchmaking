using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking;
using Rovio.MatchMaking.Net;
using Rovio.MatchMaking.Repositories;
using Rovio.MatchMaking.Repositories.Data;

var builder = WebApplication.CreateBuilder(args);

// Register the DbContext with DI
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run("http://localhost:5003");