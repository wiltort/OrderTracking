using System.Linq;
using Confluent.Kafka;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderTracking.Application.Commands.CreateOrder;
using OrderTracking.Application.Commands.UpdateOrderStatus;
using OrderTracking.Application.DTOs;
using OrderTracking.Application.Mappings;
using OrderTracking.Application.Queries.GetOrderById;
using OrderTracking.Application.Queries.GetOrders;
using OrderTracking.Domain.Exceptions;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Infrastructure.Data;
using OrderTracking.Infrastructure.Messaging.Kafka;
using OrderTracking.Infrastructure.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(WebApplication.CreateBuilder(args).Configuration)
    .CreateLogger();

try
{
    Log.Information("Starting OrderTracking API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
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

    // EF Core + PostgreSQL
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
        ));

    // Kafka
    builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
    builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

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
            .AddAspNetCoreInstrumentation());

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Exception handling middleware
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (OrderNotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    });

    // Pipeline
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // === Endpoints ===

    // Create order
    app.MapPost("/orders", async (CreateOrderRequest request, IMediator mediator) =>
    {
        var command = new CreateOrderCommand(request.Description);
        var result = await mediator.Send(command);
        return Results.Created($"/orders/{result.Id}", result);
    })
    .WithName("CreateOrder")
    .WithTags("Orders");

    // Get all orders
    app.MapGet("/orders", async (IMediator mediator) =>
    {
        var orders = await mediator.Send(new GetOrdersQuery());
        return Results.Ok(orders);
    })
    .WithName("GetOrders")
    .WithTags("Orders");

    // Get order by ID
    app.MapGet("/orders/{id}", async (Guid id, IMediator mediator) =>
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id));
        return result is null ? Results.NotFound() : Results.Ok(result);
    })
    .WithName("GetOrderById")
    .WithTags("Orders");

    // Update order status
    app.MapPut("/orders/{id}/status", async (
        Guid id,
        UpdateOrderStatusRequest request,
        IMediator mediator) =>
    {
        await mediator.Send(new UpdateOrderStatusCommand(id, request.Status));
        return Results.NoContent();
    })
    .WithName("UpdateOrderStatus")
    .WithTags("Orders");

    // Delete order (soft delete via Cancelled status)
    app.MapDelete("/orders/{id}", async (Guid id, IMediator mediator) =>
    {
        var order = await mediator.Send(new GetOrderByIdQuery(id));
        if (order is null)
            return Results.NotFound();

        await mediator.Send(new UpdateOrderStatusCommand(id, "Cancelled"));
        return Results.NoContent();
    })
    .WithName("DeleteOrder")
    .WithTags("Orders");

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
