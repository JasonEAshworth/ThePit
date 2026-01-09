using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.DataAccess.Repositories;
using ThePit.Services.Commands.Invoice;
using ThePit.Services.Commands.Payments;
using ThePit.Services.Queries;
using ThePit.Services.Queries.Payments;
using Xunit;

namespace ThePitApi.Tests;

public class CqrsIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ThePitDbContext _context;
    private readonly IMediator _mediator;

    public CqrsIntegrationTests()
    {
        var services = new ServiceCollection();

        // Configure in-memory database
        services.AddDbContext<ThePitDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Register repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Register MediatR with handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetInvoiceByIdQuery>());

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ThePitDbContext>();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }

    #region Invoice Query Tests

    [Fact]
    public async Task GetAllInvoicesQuery_ShouldReturnAllInvoices()
    {
        // Arrange
        _context.Invoices.AddRange(
            new Invoice { InvoiceNumber = "INV-001", Amount = 100m, CustomerName = "A", CustomerEmail = "a@test.com", CreatedAt = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), Status = "Pending" },
            new Invoice { InvoiceNumber = "INV-002", Amount = 200m, CustomerName = "B", CustomerEmail = "b@test.com", CreatedAt = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(30), Status = "Pending" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new GetAllInvoicesQuery());

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetInvoiceByIdQuery_ShouldReturnInvoice_WhenExists()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-TEST",
            Amount = 150m,
            CustomerName = "Test Customer",
            CustomerEmail = "test@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new GetInvoiceByIdQuery(invoice.Id));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-TEST", result.InvoiceNumber);
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public async Task GetInvoiceByIdQuery_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _mediator.Send(new GetInvoiceByIdQuery(999));

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Invoice Command Tests

    [Fact]
    public async Task CreateInvoiceCommand_ShouldCreateInvoice()
    {
        // Arrange
        var command = new CreateInvoiceCommand("INV-NEW", 500m, DateTime.UtcNow.AddDays(30));

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("INV-NEW", result.InvoiceNumber);
        Assert.Equal(500m, result.Amount);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task CreateInvoiceCommand_ShouldThrow_WhenInvalidData()
    {
        // Arrange - empty invoice number
        var command = new CreateInvoiceCommand("", 500m, DateTime.UtcNow.AddDays(30));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _mediator.Send(command));
    }

    [Fact]
    public async Task UpdateInvoiceCommand_ShouldUpdateInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-UPD",
            Amount = 100m,
            CustomerName = "Original",
            CustomerEmail = "orig@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var command = new UpdateInvoiceCommand(invoice.Id, null, 999m, null, "Paid");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.Equal(999m, result.Amount);
        Assert.Equal("Paid", result.Status);
    }

    [Fact]
    public async Task DeleteInvoiceCommand_ShouldDeleteInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-DEL",
            Amount = 100m,
            CustomerName = "Delete",
            CustomerEmail = "del@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new DeleteInvoiceCommand(invoice.Id));

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Invoices.FindAsync(invoice.Id));
    }

    [Fact]
    public async Task DeleteInvoiceCommand_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _mediator.Send(new DeleteInvoiceCommand(999));

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Payment Query Tests

    [Fact]
    public async Task GetAllPaymentsQuery_ShouldReturnAllPayments()
    {
        // Arrange - first create an invoice
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY",
            Amount = 500m,
            CustomerName = "Pay Test",
            CustomerEmail = "pay@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        _context.Payments.AddRange(
            new Payment { InvoiceId = invoice.Id, Amount = 100m, PaymentMethod = "Card", TransactionId = "TXN-1", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new Payment { InvoiceId = invoice.Id, Amount = 200m, PaymentMethod = "Bank", TransactionId = "TXN-2", Status = "Completed", PaymentDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new GetAllPaymentsQuery());

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetPaymentByIdQuery_ShouldReturnPayment_WhenExists()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PAY2",
            Amount = 300m,
            CustomerName = "Test",
            CustomerEmail = "test@test.com",
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
            PaymentMethod = "Card",
            TransactionId = "TXN-TEST",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new GetPaymentByIdQuery(payment.Id));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150m, result.Amount);
        Assert.Equal("Card", result.PaymentMethod);
    }

    [Fact]
    public async Task GetPaymentByIdQuery_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _mediator.Send(new GetPaymentByIdQuery(999));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPaymentByTransactionIdQuery_ShouldReturnPayment()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-TXN",
            Amount = 400m,
            CustomerName = "TXN Test",
            CustomerEmail = "txn@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = 200m,
            PaymentMethod = "Card",
            TransactionId = "TXN-UNIQUE-123",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new GetPaymentByTransactionIdQuery("TXN-UNIQUE-123"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TXN-UNIQUE-123", result.TransactionId);
    }

    [Fact]
    public async Task GetPaymentsByInvoiceQuery_ShouldReturnPaymentsForInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-MULTI",
            Amount = 1000m,
            CustomerName = "Multi",
            CustomerEmail = "multi@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        _context.Payments.AddRange(
            new Payment { InvoiceId = invoice.Id, Amount = 300m, PaymentMethod = "Card", TransactionId = "TXN-A", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new Payment { InvoiceId = invoice.Id, Amount = 400m, PaymentMethod = "Bank", TransactionId = "TXN-B", Status = "Completed", PaymentDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _mediator.Send(new GetPaymentsByInvoiceQuery(invoice.Id));

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(invoice.Id, p.InvoiceId));
    }

    #endregion

    #region Payment Command Tests

    [Fact]
    public async Task CreatePaymentCommand_ShouldCreatePayment()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-CREATE",
            Amount = 500m,
            CustomerName = "Create Test",
            CustomerEmail = "create@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var command = new CreatePaymentCommand(invoice.Id, 250m, "Card");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal(invoice.Id, result.InvoiceId);
        Assert.Equal(250m, result.Amount);
        Assert.Equal("Card", result.PaymentMethod);
        Assert.NotEmpty(result.TransactionId);
    }

    [Fact]
    public async Task ProcessPaymentCommand_ShouldProcessPayment()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-PROCESS",
            Amount = 500m,
            CustomerName = "Process Test",
            CustomerEmail = "process@test.com",
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending"
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var command = new ProcessPaymentCommand(invoice.Id, 500m, "Card");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task DeletePaymentCommand_ShouldDeletePayment()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-DEL-PAY",
            Amount = 200m,
            CustomerName = "Delete Pay",
            CustomerEmail = "delpay@test.com",
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
        var result = await _mediator.Send(new DeletePaymentCommand(payment.Id));

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Payments.FindAsync(payment.Id));
    }

    [Fact]
    public async Task DeletePaymentCommand_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _mediator.Send(new DeletePaymentCommand(999));

        // Assert
        Assert.False(result);
    }

    #endregion
}
