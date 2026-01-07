using Microsoft.EntityFrameworkCore;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Interfaces;
using ThePit.DataAccess.Repositories;
using ThePit.Services.Interfaces;
using ThePit.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
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

// Configure DbContext with InMemory provider for development
builder.Services.AddDbContext<ThePitDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase("ThePitDb");
    }
    // Production configuration would use a real database connection string
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
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
