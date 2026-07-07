using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderTracking.API.Middleware;
using OrderTracking.API.Services;
using OrderTracking.Application.Commands.CreateOrder;
using OrderTracking.Application.Interfaces;
using OrderTracking.Application.Mappings;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Infrastructure.Data;
using OrderTracking.Infrastructure.Messaging.Kafka;
using OrderTracking.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog((context, configuration) =>
{
    if (!context.HostingEnvironment.IsDevelopment())
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
            .WriteTo.Console()
            .WriteTo.File("logs/order-tracking-.txt", rollingInterval: RollingInterval.Day);
    }
    else
    {
        configuration
            .WriteTo.Console();
    }
});

Log.Information("Starting OrderTracking API");

try
{
    // EF Core + PostgreSQL
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
        ));

    // Kafka
    builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
    builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
    builder.Services.AddHostedService<KafkaConsumerService>();

    // SignalR
    builder.Services.AddSignalR();
    builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

    // Repositories
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();

    // AutoMapper
    builder.Services.AddAutoMapper(typeof(OrderProfile));

    // MediatR
    builder.Services.AddMediatR(typeof(CreateOrderCommand));

    // OpenTelemetry
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("order-tracking-api"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddConsoleExporter());

    // Controllers
    builder.Services.AddControllers();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            corsPolicyBuilder =>
        {
            corsPolicyBuilder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Exception handling middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Pipeline
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.MapControllers();
    app.MapHub<OrderTracking.API.Hubs.OrderHub>("/hubs/order");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
