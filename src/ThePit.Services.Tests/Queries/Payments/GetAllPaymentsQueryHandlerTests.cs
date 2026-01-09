using Moq;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Queries.Payments;
using Xunit;

namespace ThePit.Services.Tests.Queries.Payments;

public class GetAllPaymentsQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly GetAllPaymentsQueryHandler _handler;

    public GetAllPaymentsQueryHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _handler = new GetAllPaymentsQueryHandler(_paymentRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAllPayments()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { Id = 1, TransactionId = "TXN-001", InvoiceId = 100, Amount = 50.00m, PaymentMethod = "CreditCard", Status = "Completed", PaymentDate = DateTime.UtcNow },
            new() { Id = 2, TransactionId = "TXN-002", InvoiceId = 101, Amount = 75.00m, PaymentMethod = "PayPal", Status = "Pending", PaymentDate = DateTime.UtcNow }
        };
        _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);

        var query = new GetAllPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Handle_WhenNoPayments_ReturnsEmptyCollection()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Payment>());
        var query = new GetAllPaymentsQuery();

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
        _paymentRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(payments);

        var query = new GetAllPaymentsQuery();

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
        Assert.Throws<ArgumentNullException>(() => new GetAllPaymentsQueryHandler(null!));
    }
}
