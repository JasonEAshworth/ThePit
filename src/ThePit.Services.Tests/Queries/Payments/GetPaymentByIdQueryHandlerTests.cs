using Moq;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Queries.Payments;
using Xunit;

namespace ThePit.Services.Tests.Queries.Payments;

public class GetPaymentByIdQueryHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly GetPaymentByIdQueryHandler _handler;

    public GetPaymentByIdQueryHandlerTests()
    {
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _handler = new GetPaymentByIdQueryHandler(_paymentRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsPaymentDto()
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

        var query = new GetPaymentByIdQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("TXN-001", result.TransactionId);
        Assert.Equal(100, result.InvoiceId);
        Assert.Equal(50.00m, result.Amount);
        Assert.Equal("CreditCard", result.PaymentMethod);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task Handle_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var queryZero = new GetPaymentByIdQuery(0);
        var queryNegative = new GetPaymentByIdQuery(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(queryZero, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(queryNegative, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _paymentRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Payment?)null);
        var query = new GetPaymentByIdQuery(999);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetPaymentByIdQueryHandler(null!));
    }
}
