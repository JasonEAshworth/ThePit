using Microsoft.EntityFrameworkCore;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Repositories;
using Xunit;

namespace ThePitApi.Tests;

public class PaymentRepositoryTests : IDisposable
{
    private readonly ThePitDbContext _context;
    private readonly PaymentRepository _repository;

    public PaymentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ThePitDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ThePitDbContext(options);
        _repository = new PaymentRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPayment()
    {
        // Arrange
        var payment = new Payment
        {
            InvoiceId = 1,
            Amount = 50.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Credit Card",
            TransactionId = "TXN-001",
            Status = "Completed"
        };

        // Act
        var result = await _repository.CreateAsync(payment);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("TXN-001", result.TransactionId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPayment_WhenExists()
    {
        // Arrange
        var payment = new Payment
        {
            InvoiceId = 1,
            Amount = 75.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "PayPal",
            TransactionId = "TXN-002",
            Status = "Completed"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(payment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TXN-002", result.TransactionId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTransactionIdAsync_ShouldReturnPayment()
    {
        // Arrange
        var payment = new Payment
        {
            InvoiceId = 2,
            Amount = 100.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Bank Transfer",
            TransactionId = "TXN-003",
            Status = "Pending"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTransactionIdAsync("TXN-003");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100.00m, result.Amount);
    }

    [Fact]
    public async Task GetByTransactionIdAsync_ShouldThrow_WhenEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetByTransactionIdAsync(""));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPayments()
    {
        // Arrange
        _context.Payments.AddRange(
            new Payment { InvoiceId = 1, Amount = 50m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Card", TransactionId = "TXN-A", Status = "Done" },
            new Payment { InvoiceId = 2, Amount = 75m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Cash", TransactionId = "TXN-B", Status = "Done" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByInvoiceIdAsync_ShouldReturnMatchingPayments()
    {
        // Arrange
        _context.Payments.AddRange(
            new Payment { InvoiceId = 5, Amount = 50m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Card", TransactionId = "TXN-X", Status = "Done" },
            new Payment { InvoiceId = 5, Amount = 25m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Card", TransactionId = "TXN-Y", Status = "Done" },
            new Payment { InvoiceId = 6, Amount = 100m, PaymentDate = DateTime.UtcNow, PaymentMethod = "Card", TransactionId = "TXN-Z", Status = "Done" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByInvoiceIdAsync(5);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyPayment()
    {
        // Arrange
        var payment = new Payment
        {
            InvoiceId = 1,
            Amount = 100m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Card",
            TransactionId = "TXN-UPD",
            Status = "Pending"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        payment.Status = "Completed";
        payment.Amount = 150m;
        var result = await _repository.UpdateAsync(payment);

        // Assert
        Assert.Equal("Completed", result.Status);
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var payment = new Payment { Id = 999, InvoiceId = 1, TransactionId = "FAKE" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.UpdateAsync(payment));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemovePayment()
    {
        // Arrange
        var payment = new Payment
        {
            InvoiceId = 1,
            Amount = 50m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "Card",
            TransactionId = "TXN-DEL",
            Status = "Pending"
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(payment.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Payments.FindAsync(payment.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }
}
