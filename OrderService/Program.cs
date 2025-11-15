using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderService.Data;
using OrderService.Domain.Interfaces;
using OrderService.Repositories;
using OrderService.Services;
using OrderService.Validators;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((ctx, log) =>
{
    log.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console()
       .WriteTo.File("logs/order-service-.txt", rollingInterval: RollingInterval.Day);
});

// Add services to the container.
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Service API",
        Version = "v1"
    });
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// EF InMemory
builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseInMemoryDatabase("OrdersDb");
});

// Dependency Injection
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();

// HttpClient for calling ProductService
builder.Services.AddHttpClient<IOrderService, OrderService.Services.OrderService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
