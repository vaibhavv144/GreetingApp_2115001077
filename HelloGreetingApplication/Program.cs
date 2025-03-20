using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Contexts;
using BusinessLayer.Service;
using RepositoryLayer.Service;
using MiddleWare.JwtHelper;
using MiddleWare.RabbitMQClient;
using StackExchange.Redis;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Info("Starting up the application");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Load NLog configuration
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddControllers();

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "GreetingApp_";
    });
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));

    // Retrieve connection string
    var connectionString = builder.Configuration.GetConnectionString("GreetingAppDB");

    Console.WriteLine($"Connection String: {connectionString}"); // Debugging output

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'GreetingAppDB' not found in appsettings.json.");
    }

    

    // Register DbContext
    builder.Services.AddDbContext<GreetingAppContext>(options =>
        options.UseSqlServer(connectionString));


    builder.Services.AddScoped<MiddleWare.Email.SMTP>();
    builder.Services.AddScoped<IRabbitMQService,RabbitMQService>();

    builder.Services.AddScoped<IGreetingBL, GreetingBL>();
    builder.Services.AddScoped<IGreetingRL, GreetingRL>();
    builder.Services.AddScoped<IUserBL, UserBL>();
    builder.Services.AddScoped<IUserRL, UserRL>();

    builder.Services.AddSingleton<JwtTokenHelper>();


    // Configure Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Registration API", Version = "v1" });
    });

    var app = builder.Build();

    // Enable Swagger UI
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Registration API v1"));
    }

    // Configure the HTTP request pipeline
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application startup failed.");
    throw;
}
finally
{
    LogManager.Shutdown();
}