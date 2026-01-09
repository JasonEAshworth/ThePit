using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Entities;
using ThePit.Services.DTOs;
using ThePit.Services.Interfaces;
using ThePitApi.Controllers;
using Xunit;

namespace ThePitApi.Tests;

public class ControllerCqrsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly ThePitDbContext _context;

    public ControllerCqrsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ThePitDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database with unique name per test
                services.AddDbContext<ThePitDbContext>(options =>
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ThePitDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _client.Dispose();
    }

    #region InvoiceController Tests

    [Fact]
    public async Task InvoiceController_GetAll_ReturnsOkWithInvoices()
    {
        // Arrange
        _context.Invoices.Add(new Invoice
        {
            InvoiceNumber = "INV-HTTP-001",
            Amount = 100m,
            CustomerName = "HTTP Test",
            CustomerEmail = "http@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        });
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/invoice");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var invoices = await response.Content.ReadFromJsonAsync<IEnumerable<InvoiceDto>>();
        Assert.NotNull(invoices);
        Assert.Contains(invoices, i => i.InvoiceNumber == "INV-HTTP-001");
    }

    [Fact]
    public async Task InvoiceController_GetById_ReturnsInvoice_WhenExists()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-HTTP-GET",
            Amount = 200m,
            CustomerName = "Get Test",
            CustomerEmail = "get@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/invoice/{invoice.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InvoiceDto>();
        Assert.NotNull(result);
        Assert.Equal("INV-HTTP-GET", result.InvoiceNumber);
    }

    [Fact]
    public async Task InvoiceController_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync("/api/invoice/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InvoiceController_Create_ReturnsCreatedInvoice()
    {
        // Arrange
        var createDto = new CreateInvoiceDto("INV-HTTP-NEW", 350m, DateTime.UtcNow.AddDays(30));

        // Act
        var response = await _client.PostAsJsonAsync("/api/invoice", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InvoiceDto>();
        Assert.NotNull(result);
        Assert.Equal("INV-HTTP-NEW", result.InvoiceNumber);
        Assert.Equal(350m, result.Amount);
    }

    [Fact]
    public async Task InvoiceController_Update_ReturnsUpdatedInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-HTTP-UPD",
            Amount = 100m,
            CustomerName = "Update Test",
            CustomerEmail = "update@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateInvoiceDto(null, 999m, null, "Paid");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/invoice/{invoice.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InvoiceDto>();
        Assert.NotNull(result);
        Assert.Equal(999m, result.Amount);
        Assert.Equal("Paid", result.Status);
    }

    [Fact]
    public async Task InvoiceController_Delete_ReturnsNoContent()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-HTTP-DEL",
            Amount = 50m,
            CustomerName = "Delete Test",
            CustomerEmail = "delete@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/invoice/{invoice.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region PaymentController Tests

    [Fact]
    public async Task PaymentController_GetAll_ReturnsOkWithPayments()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY-HTTP",
            Amount = 500m,
            CustomerName = "Pay HTTP",
            CustomerEmail = "payhttp@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        _context.Payments.Add(new Payment
        {
            InvoiceId = invoice.Id,
            Amount = 100m,
            PaymentMethod = "Card",
            TransactionId = "TXN-HTTP-001",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/payment");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payments = await response.Content.ReadFromJsonAsync<IEnumerable<PaymentDto>>();
        Assert.NotNull(payments);
        Assert.Contains(payments, p => p.TransactionId == "TXN-HTTP-001");
    }

    [Fact]
    public async Task PaymentController_GetById_ReturnsPayment_WhenExists()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY-GET",
            Amount = 300m,
            CustomerName = "Pay Get",
            CustomerEmail = "payget@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = 150m,
            PaymentMethod = "Bank",
            TransactionId = "TXN-HTTP-GET",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/payment/{payment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentDto>();
        Assert.NotNull(result);
        Assert.Equal("Bank", result.PaymentMethod);
    }

    [Fact]
    public async Task PaymentController_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync("/api/payment/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PaymentController_GetByInvoiceId_ReturnsPayments()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY-INV",
            Amount = 600m,
            CustomerName = "Pay Invoice",
            CustomerEmail = "payinv@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        _context.Payments.AddRange(
            new Payment { InvoiceId = invoice.Id, Amount = 200m, PaymentMethod = "Card", TransactionId = "TXN-INV-1", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new Payment { InvoiceId = invoice.Id, Amount = 300m, PaymentMethod = "Bank", TransactionId = "TXN-INV-2", Status = "Completed", PaymentDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/payment/invoice/{invoice.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payments = await response.Content.ReadFromJsonAsync<IEnumerable<PaymentDto>>();
        Assert.NotNull(payments);
        Assert.Equal(2, payments.Count());
    }

    [Fact]
    public async Task PaymentController_Create_ReturnsCreatedPayment()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY-CREATE",
            Amount = 400m,
            CustomerName = "Pay Create",
            CustomerEmail = "paycreate@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var createDto = new CreatePaymentDto
        {
            InvoiceId = invoice.Id,
            Amount = 200m,
            PaymentMethod = "Card"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payment", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentDto>();
        Assert.NotNull(result);
        Assert.Equal(200m, result.Amount);
        Assert.Equal("Card", result.PaymentMethod);
    }

    [Fact]
    public async Task PaymentController_ProcessPayment_ReturnsProcessedPayment()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY-PROC",
            Amount = 500m,
            CustomerName = "Pay Process",
            CustomerEmail = "payproc@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var processRequest = new ProcessPaymentRequest(invoice.Id, 500m, "Card");

        // Act
        var response = await _client.PostAsJsonAsync("/api/payment/process", processRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentDto>();
        Assert.NotNull(result);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task PaymentController_Delete_ReturnsNoContent()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY-DEL",
            Amount = 200m,
            CustomerName = "Pay Delete",
            CustomerEmail = "paydel@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = 100m,
            PaymentMethod = "Card",
            TransactionId = "TXN-DEL",
            Status = "Pending",
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/payment/{payment.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion
}
