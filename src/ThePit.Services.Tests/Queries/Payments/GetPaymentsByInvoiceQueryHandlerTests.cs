using Moq;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Queries.Payments;
using Xunit;

namespace ThePit.Services.Tests.Queries.Payments;

public class GetPaymentsByInvoiceQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly GetPaymentsByInvoiceQueryHandler _handler;

    public GetPaymentsByInvoiceQueryHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentsByInvoiceQueryHandler(_paymentRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidInvoiceId_ReturnsPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new() { Id = 2, TransactionId = "TXN-002", InvoiceId = 100, Amount = 25.00m, PaymentMethod = "PayPal", Status = "Pending", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetByInvoiceIdAsync(100)).ReturnsAsync(payments);

        var query = new GetPaymentsByInvoiceQuery(100);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(100, p.InvoiceId));
    }

    [Fact]
    public async Task Handle_WithInvalidInvoiceId_ThrowsArgumentException()
    {
        // Arrange
        var queryZero = new GetPaymentsByInvoiceQuery(0);
        var queryNegative = new GetPaymentsByInvoiceQuery(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(queryZero, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(queryNegative, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNoPaymentsForInvoice_ReturnsEmptyCollection()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetByInvoiceIdAsync(999)).ReturnsAsync(new List<Payment>());
        var query = new GetPaymentsByInvoiceQuery(999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var paymentDate = DateTime.UtcNow;
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = paymentDate }
        };
        _paymentRepoMock.Setup(r => r.GetByInvoiceIdAsync(100)).ReturnsAsync(payments);

        var query = new GetPaymentsByInvoiceQuery(100);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).First();

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("TXN-001", result.TransactionId);
        Assert.Equal(100, result.InvoiceId);
        Assert.Equal(50.00m, result.Amount);
        Assert.Equal("CreditCard", result.PaymentMethod);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(paymentDate, result.PaymentDate);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetPaymentsByInvoiceQueryHandler(null!));
    }
}
