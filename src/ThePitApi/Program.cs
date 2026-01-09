using Microsoft.EntityFrameworkCore;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Interfaces;
using ThePit.DataAccess.Repositories;
using ThePit.Services.Interfaces;
using ThePit.Services.Queries.Payments;
using ThePit.Services.Services;
using ThePit.Services.Queries;

var builder = WebApplication.CreateBuilder(args);

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetInvoiceByIdQuery>());

// Add services to the container
builder.Services.AddDbContext<ThePitDbContext>(options =>
    options.UseInMemoryDatabase("ThePitDb"));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// DEPRECATED: Old service layer - use CQRS handlers instead
#pragma warning disable CS0618 // Type or member is obsolete
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
#pragma warning restore CS0618

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "ThePit API",
        Description = "A .NET REST API for ThePit"
    });
});

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ThePitDbContext>(options =>
{
    if (string.IsNullOrEmpty(connectionString))
    {
        // Fallback to InMemory for testing
        options.UseInMemoryDatabase("ThePitDb");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ThePit API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
