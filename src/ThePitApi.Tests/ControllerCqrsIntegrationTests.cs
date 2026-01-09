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
                // Remove existing DbContext registrations
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ThePitDbContext>) ||
                    d.ServiceType == typeof(ThePitDbContext)).ToList();
                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                // Add in-memory database with unique name per test class
                services.AddDbContext<ThePitDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString()));
            });
        });

        _client = _factory.CreateClient();
        // Get scope from the CONFIGURED factory (not the original)
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
        // Arrange - use the client to create data via POST instead of direct DB access
        var createDto = new CreateInvoiceDto("INV-HTTP-001", 100m, DateTime.UtcNow.AddDays(30));
        await _client.PostAsJsonAsync("/api/invoice", createDto);

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
        // Arrange - create via API
        var createDto = new CreateInvoiceDto("INV-HTTP-GET", 200m, DateTime.UtcNow.AddDays(30));
        var createResponse = await _client.PostAsJsonAsync("/api/invoice", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        // Act
        var response = await _client.GetAsync($"/api/invoice/{created!.Id}");

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
        // Arrange - create via API
        var createDto = new CreateInvoiceDto("INV-HTTP-UPD", 100m, DateTime.UtcNow.AddDays(30));
        var createResponse = await _client.PostAsJsonAsync("/api/invoice", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        var updateDto = new UpdateInvoiceDto(null, 999m, null, "Paid");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/invoice/{created!.Id}", updateDto);

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
        // Arrange - create via API
        var createDto = new CreateInvoiceDto("INV-HTTP-DEL", 50m, DateTime.UtcNow.AddDays(30));
        var createResponse = await _client.PostAsJsonAsync("/api/invoice", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/invoice/{created!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region PaymentController Tests

    [Fact]
    public async Task PaymentController_GetAll_ReturnsOkWithPayments()
    {
        // Arrange - create invoice then payment via API
        var invoiceDto = new CreateInvoiceDto("INV-PAY-HTTP", 500m, DateTime.UtcNow.AddDays(30));
        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoice", invoiceDto);
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        var paymentDto = new CreatePaymentDto { InvoiceId = invoice!.Id, Amount = 100m, PaymentMethod = "Card" };
        await _client.PostAsJsonAsync("/api/payment", paymentDto);

        // Act
        var response = await _client.GetAsync("/api/payment");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payments = await response.Content.ReadFromJsonAsync<IEnumerable<PaymentDto>>();
        Assert.NotNull(payments);
        Assert.NotEmpty(payments);
    }

    [Fact]
    public async Task PaymentController_GetById_ReturnsPayment_WhenExists()
    {
        // Arrange - create invoice then payment via API
        var invoiceDto = new CreateInvoiceDto("INV-PAY-GET", 300m, DateTime.UtcNow.AddDays(30));
        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoice", invoiceDto);
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        var paymentDto = new CreatePaymentDto { InvoiceId = invoice!.Id, Amount = 150m, PaymentMethod = "Bank" };
        var paymentResponse = await _client.PostAsJsonAsync("/api/payment", paymentDto);
        var payment = await paymentResponse.Content.ReadFromJsonAsync<PaymentDto>();

        // Act
        var response = await _client.GetAsync($"/api/payment/{payment!.Id}");

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
        // Arrange - create invoice then payments via API
        var invoiceDto = new CreateInvoiceDto("INV-PAY-INV", 600m, DateTime.UtcNow.AddDays(30));
        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoice", invoiceDto);
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        await _client.PostAsJsonAsync("/api/payment", new CreatePaymentDto { InvoiceId = invoice!.Id, Amount = 200m, PaymentMethod = "Card" });
        await _client.PostAsJsonAsync("/api/payment", new CreatePaymentDto { InvoiceId = invoice.Id, Amount = 300m, PaymentMethod = "Bank" });

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
        // Arrange - create invoice first
        var invoiceDto = new CreateInvoiceDto("INV-PAY-CREATE", 400m, DateTime.UtcNow.AddDays(30));
        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoice", invoiceDto);
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        var createDto = new CreatePaymentDto { InvoiceId = invoice!.Id, Amount = 200m, PaymentMethod = "Card" };

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
        // Arrange - create invoice first
        var invoiceDto = new CreateInvoiceDto("INV-PAY-PROC", 500m, DateTime.UtcNow.AddDays(30));
        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoice", invoiceDto);
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        var processRequest = new ProcessPaymentRequest(invoice!.Id, 500m, "Card");

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
        // Arrange - create invoice then payment via API
        var invoiceDto = new CreateInvoiceDto("INV-PAY-DEL", 200m, DateTime.UtcNow.AddDays(30));
        var invoiceResponse = await _client.PostAsJsonAsync("/api/invoice", invoiceDto);
        var invoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceDto>();

        var paymentDto = new CreatePaymentDto { InvoiceId = invoice!.Id, Amount = 100m, PaymentMethod = "Card" };
        var paymentResponse = await _client.PostAsJsonAsync("/api/payment", paymentDto);
        var payment = await paymentResponse.Content.ReadFromJsonAsync<PaymentDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/payment/{payment!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion
}
