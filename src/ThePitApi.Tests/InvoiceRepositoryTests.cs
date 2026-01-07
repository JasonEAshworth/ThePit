using Microsoft.EntityFrameworkCore;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Repositories;
using Xunit;

namespace ThePitApi.Tests;

public class InvoiceRepositoryTests : IDisposable
{
    private readonly ThePitDbContext _context;
    private readonly InvoiceRepository _repository;

    public InvoiceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ThePitDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ThePitDbContext(options);
        _repository = new InvoiceRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-001",
            Amount = 100.50m,
            CustomerName = "John Doe",
            CustomerEmail = "john@example.com"
        };

        // Act
        var result = await _repository.CreateAsync(invoice);

        // Assert
        Assert.NotEqual(0, result.Id);
        Assert.Equal("INV-001", result.InvoiceNumber);
        Assert.NotEqual(default, result.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnInvoice_WhenExists()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-002",
            Amount = 200.00m,
            CustomerName = "Jane Doe",
            CustomerEmail = "jane@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(invoice.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-002", result.InvoiceNumber);
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
    public async Task GetByInvoiceNumberAsync_ShouldReturnInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-003",
            Amount = 300.00m,
            CustomerName = "Bob Smith",
            CustomerEmail = "bob@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByInvoiceNumberAsync("INV-003");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(300.00m, result.Amount);
    }

    [Fact]
    public async Task GetByInvoiceNumberAsync_ShouldThrow_WhenEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.GetByInvoiceNumberAsync(""));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllInvoices()
    {
        // Arrange
        _context.Invoices.AddRange(
            new Invoice { InvoiceNumber = "INV-A", Amount = 100m, CustomerName = "A", CustomerEmail = "a@test.com", CreatedAt = DateTime.UtcNow },
            new Invoice { InvoiceNumber = "INV-B", Amount = 200m, CustomerName = "B", CustomerEmail = "b@test.com", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByCustomerEmailAsync_ShouldReturnMatchingInvoices()
    {
        // Arrange
        _context.Invoices.AddRange(
            new Invoice { InvoiceNumber = "INV-X", Amount = 100m, CustomerName = "X", CustomerEmail = "same@test.com", CreatedAt = DateTime.UtcNow },
            new Invoice { InvoiceNumber = "INV-Y", Amount = 200m, CustomerName = "Y", CustomerEmail = "same@test.com", CreatedAt = DateTime.UtcNow },
            new Invoice { InvoiceNumber = "INV-Z", Amount = 300m, CustomerName = "Z", CustomerEmail = "other@test.com", CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCustomerEmailAsync("same@test.com");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-UPD",
            Amount = 100m,
            CustomerName = "Original",
            CustomerEmail = "original@test.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        invoice.Amount = 999m;
        invoice.CustomerName = "Updated";
        var result = await _repository.UpdateAsync(invoice);

        // Assert
        Assert.Equal(999m, result.Amount);
        Assert.Equal("Updated", result.CustomerName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var invoice = new Invoice { Id = 999, InvoiceNumber = "FAKE", Amount = 0m };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.UpdateAsync(invoice));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveInvoice()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-DEL",
            Amount = 100m,
            CustomerName = "Delete Me",
            CustomerEmail = "delete@test.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(invoice.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Invoices.FindAsync(invoice.Id));
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
