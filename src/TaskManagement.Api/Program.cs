using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Persistence;

using FluentValidation;
using FluentValidation.AspNetCore;
using TaskManagement.Application.Validators;
using TaskManagement.Api.Middlewares;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


// Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config
        // Minimum log level
        .MinimumLevel.Information()

        // Reduce ASP.NET Core internal logs
        .MinimumLevel.Override(
            "Microsoft",
            LogEventLevel.Warning)

        .MinimumLevel.Override(
            "System",
            LogEventLevel.Warning)

        // Read properties from LogContext
        .Enrich.FromLogContext()

        // Console logging
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] " +
            "[CorrelationId: {CorrelationId}] " +
            "[TenantId: {TenantId}] " +
            "{Message:lj}{NewLine}{Exception}")

        // File logging
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
            "[CorrelationId: {CorrelationId}] " +
            "[TenantId: {TenantId}] " +
            "{Message:lj}{NewLine}{Exception}");
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateTaskRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<TenantHeaderOperationFilter>();
});

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Global exception logging middleware (optional but recommended)
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TenantMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.MapControllers();


app.Run();


