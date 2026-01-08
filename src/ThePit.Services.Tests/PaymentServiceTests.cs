using Moq;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;
using ThePit.Services.Services;
using Xunit;

namespace ThePit.Services.Tests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _service = new PaymentService(_paymentRepoMock.Object, _invoiceRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsPaymentDto()
    {
        // Arrange
        var payment = new Payment
        {
            Id = 1,
            TransactionId = "TXN-001",
            InvoiceId = 100,
            Amount = 50.00m,
            PaymentMethod = "CreditCard",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };
        _paymentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("TXN-001", result.TransactionId);
        Assert.Equal(100, result.InvoiceId);
        Assert.Equal(50.00m, result.Amount);
        Assert.Equal("CreditCard", result.PaymentMethod);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(-1));
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Payment?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new() { Id = 2, TransactionId = "TXN-002", InvoiceId = 101, Amount = 75.00m, PaymentMethod = "PayPal", Status = "Pending", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByInvoiceIdAsync_WithValidId_ReturnsPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetByInvoiceIdAsync(100)).ReturnsAsync(payments);

        // Act
        var result = await _service.GetByInvoiceIdAsync(100);

        // Assert
        Assert.Single(result);
        Assert.Equal(100, result.First().InvoiceId);
    }

    [Fact]
    public async Task GetByInvoiceIdAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByInvoiceIdAsync(0));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithValidArgs_CreatesPaymentAndUpdatesInvoice()
    {
        // Arrange
        var invoice = new Invoice { Id = 100, Status = "Pending", Amount = 50.00m };
        var createdPayment = new Payment
        {
            Id = 1,
            TransactionId = "TXN-001",
            InvoiceId = 100,
            Amount = 50.00m,
            PaymentMethod = "CreditCard",
            Status = "Processing",
            PaymentDate = DateTime.UtcNow
        };
        var processedPayment = new Payment
        {
            Id = 1,
            TransactionId = "TXN-001",
            InvoiceId = 100,
            Amount = 50.00m,
            PaymentMethod = "CreditCard",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(invoice);
        _paymentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Payment>())).ReturnsAsync(createdPayment);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync(processedPayment);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>())).ReturnsAsync(invoice);

        // Act
        var result = await _service.ProcessPaymentAsync(100, 50.00m, "CreditCard");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.InvoiceId);
        Assert.Equal(50.00m, result.Amount);
        Assert.Equal("Completed", result.Status);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(i => i.Status == "Paid")), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidInvoiceId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(0, 50.00m, "CreditCard"));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidAmount_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(100, 0, "CreditCard"));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithEmptyPaymentMethod_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(100, 50.00m, ""));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenInvoiceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Invoice?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessPaymentAsync(999, 50.00m, "CreditCard"));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenInvoiceAlreadyPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = new Invoice { Id = 100, Status = "Paid" };
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(invoice);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessPaymentAsync(100, 50.00m, "CreditCard"));
    }

    [Fact]
    public void Constructor_WithNullPaymentRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PaymentService(null!, _invoiceRepoMock.Object));
    }

    [Fact]
    public void Constructor_WithNullInvoiceRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PaymentService(_paymentRepoMock.Object, null!));
    }

    [Fact]
    public async Task GetFilteredAsync_WithNoFilters_ReturnsAllPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new() { Id = 2, TransactionId = "TXN-002", InvoiceId = 101, Amount = 75.00m, PaymentMethod = "PayPal", Status = "Pending", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetFilteredAsync(null, null)).ReturnsAsync(payments);

        // Act
        var result = await _service.GetFilteredAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFilteredAsync_WithStatusFilter_ReturnsFilteredPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetFilteredAsync("Completed", null)).ReturnsAsync(payments);

        // Act
        var result = await _service.GetFilteredAsync("Completed");

        // Assert
        Assert.Single(result);
        Assert.Equal("Completed", result.First().Status);
    }

    [Fact]
    public async Task GetFilteredAsync_WithMethodFilter_ReturnsFilteredPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 2, TransactionId = "TXN-002", InvoiceId = 101, Amount = 75.00m, PaymentMethod = "PayPal", Status = "Pending", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetFilteredAsync(null, "PayPal")).ReturnsAsync(payments);

        // Act
        var result = await _service.GetFilteredAsync(null, "PayPal");

        // Assert
        Assert.Single(result);
        Assert.Equal("PayPal", result.First().PaymentMethod);
    }

    [Fact]
    public async Task GetFilteredAsync_WithBothFilters_ReturnsFilteredPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetFilteredAsync("Completed", "CreditCard")).ReturnsAsync(payments);

        // Act
        var result = await _service.GetFilteredAsync("Completed", "CreditCard");

        // Assert
        Assert.Single(result);
        Assert.Equal("Completed", result.First().Status);
        Assert.Equal("CreditCard", result.First().PaymentMethod);
    }

    [Fact]
    public async Task GetFilteredAsync_ReturnsPaymentDtoWithTransactionId()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-12345678", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetFilteredAsync(null, null)).ReturnsAsync(payments);

        // Act
        var result = await _service.GetFilteredAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("TXN-12345678", result.First().TransactionId);
    }
}
