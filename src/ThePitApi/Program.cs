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
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetPaymentByIdQuery>());

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

// Register repositories (Scoped - one instance per request)
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Register services (Scoped - one instance per request)
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

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
