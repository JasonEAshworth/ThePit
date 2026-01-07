using Moq;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;
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
            new() { Id = 1, InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new() { Id = 2, InvoiceId = 101, Amount = 75.00m, PaymentMethod = "PayPal", Status = "Pending", PaymentDate = DateTime.UtcNow }
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
            new() { Id = 1, InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow }
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
    public async Task ProcessPaymentAsync_WithValidDto_CreatesPaymentAndUpdatesInvoice()
    {
        // Arrange
        var dto = new ProcessPaymentDto(100, 50.00m, "CreditCard");
        var invoice = new Invoice { Id = 100, Status = "Pending", Amount = 50.00m };
        var createdPayment = new Payment
        {
            Id = 1,
            InvoiceId = 100,
            Amount = 50.00m,
            PaymentMethod = "CreditCard",
            Status = "Completed",
            PaymentDate = DateTime.UtcNow
        };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(invoice);
        _paymentRepoMock.Setup(r => r.CreateAsync(It.IsAny<Payment>())).ReturnsAsync(createdPayment);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>())).ReturnsAsync(invoice);

        // Act
        var result = await _service.ProcessPaymentAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.InvoiceId);
        Assert.Equal(50.00m, result.Amount);
        Assert.Equal("Completed", result.Status);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(i => i.Status == "Paid")), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithNullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ProcessPaymentAsync(null!));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidInvoiceId_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessPaymentDto(0, 50.00m, "CreditCard");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(dto));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithInvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessPaymentDto(100, 0, "CreditCard");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(dto));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WithEmptyPaymentMethod_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessPaymentDto(100, 50.00m, "");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPaymentAsync(dto));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenInvoiceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new ProcessPaymentDto(999, 50.00m, "CreditCard");
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Invoice?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessPaymentAsync(dto));
    }

    [Fact]
    public async Task ProcessPaymentAsync_WhenInvoiceAlreadyPaid_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new ProcessPaymentDto(100, 50.00m, "CreditCard");
        var invoice = new Invoice { Id = 100, Status = "Paid" };
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(invoice);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ProcessPaymentAsync(dto));
    }

    [Fact]
    public async Task RefundAsync_WithValidPayment_RefundsAndReturnsTrue()
    {
        // Arrange
        var payment = new Payment { Id = 1, InvoiceId = 100, Status = "Completed" };
        var invoice = new Invoice { Id = 100, Status = "Paid", PaidAt = DateTime.UtcNow };

        _paymentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);
        _paymentRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync(payment);
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(invoice);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>())).ReturnsAsync(invoice);

        // Act
        var result = await _service.RefundAsync(1);

        // Assert
        Assert.True(result);
        _paymentRepoMock.Verify(r => r.UpdateAsync(It.Is<Payment>(p => p.Status == "Refunded")), Times.Once);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(i => i.Status == "Refunded")), Times.Once);
    }

    [Fact]
    public async Task RefundAsync_WhenPaymentNotFound_ReturnsFalse()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Payment?)null);

        // Act
        var result = await _service.RefundAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RefundAsync_WhenAlreadyRefunded_ThrowsInvalidOperationException()
    {
        // Arrange
        var payment = new Payment { Id = 1, Status = "Refunded" };
        _paymentRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RefundAsync(1));
    }

    [Fact]
    public async Task RefundAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.RefundAsync(0));
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
}
