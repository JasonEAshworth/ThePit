using Moq;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;
using ThePit.Services.Services;
using Xunit;

namespace ThePit.Services.Tests;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepoMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _invoiceRepoMock = new Mock<IInvoiceRepository>();
        _service = new InvoiceService(_invoiceRepoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsInvoiceDto()
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            Amount = 100.00m,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(invoice);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("INV-001", result.InvoiceNumber);
        Assert.Equal(100.00m, result.Amount);
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
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Invoice?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInvoices()
    {
        // Arrange
        var invoices = new List<Invoice>
        {
            new() { Id = 1, InvoiceNumber = "INV-001", Amount = 100.00m, Status = "Pending", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, InvoiceNumber = "INV-002", Amount = 200.00m, Status = "Paid", CreatedAt = DateTime.UtcNow }
        };
        _invoiceRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(invoices);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesInvoice()
    {
        // Arrange
        var dto = new CreateInvoiceDto("INV-001", 100.00m, DateTime.UtcNow.AddDays(30));
        var createdInvoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            Amount = 100.00m,
            DueDate = dto.DueDate,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _invoiceRepoMock.Setup(r => r.GetByInvoiceNumberAsync("INV-001")).ReturnsAsync((Invoice?)null);
        _invoiceRepoMock.Setup(r => r.CreateAsync(It.IsAny<Invoice>())).ReturnsAsync(createdInvoice);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-001", result.InvoiceNumber);
        Assert.Equal(100.00m, result.Amount);
        Assert.Equal("Pending", result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithNullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_WithEmptyInvoiceNumber_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateInvoiceDto("", 100.00m, DateTime.UtcNow.AddDays(30));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateInvoiceDto("INV-001", 0, DateTime.UtcNow.AddDays(30));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateInvoiceNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateInvoiceDto("INV-001", 100.00m, DateTime.UtcNow.AddDays(30));
        var existingInvoice = new Invoice { Id = 1, InvoiceNumber = "INV-001" };

        _invoiceRepoMock.Setup(r => r.GetByInvoiceNumberAsync("INV-001")).ReturnsAsync(existingInvoice);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task UpdateAsync_WithValidDto_UpdatesInvoice()
    {
        // Arrange
        var dto = new UpdateInvoiceDto("INV-002", 150.00m, null, "Paid");
        var existingInvoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            Amount = 100.00m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingInvoice);
        _invoiceRepoMock.Setup(r => r.GetByInvoiceNumberAsync("INV-002")).ReturnsAsync((Invoice?)null);
        _invoiceRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>())).ReturnsAsync(existingInvoice);

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        _invoiceRepoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(i =>
            i.InvoiceNumber == "INV-002" &&
            i.Amount == 150.00m &&
            i.Status == "Paid")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Arrange
        var dto = new UpdateInvoiceDto(null, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(0, dto));
    }

    [Fact]
    public async Task UpdateAsync_WithNullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(1, null!));
    }

    [Fact]
    public async Task UpdateAsync_WhenInvoiceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new UpdateInvoiceDto(null, 150.00m, null, null);
        _invoiceRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Invoice?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(999, dto));
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateInvoiceNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new UpdateInvoiceDto("INV-002", null, null, null);
        var existingInvoice = new Invoice { Id = 1, InvoiceNumber = "INV-001" };
        var anotherInvoice = new Invoice { Id = 2, InvoiceNumber = "INV-002" };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingInvoice);
        _invoiceRepoMock.Setup(r => r.GetByInvoiceNumberAsync("INV-002")).ReturnsAsync(anotherInvoice);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyInvoiceNumber_ThrowsArgumentException()
    {
        // Arrange
        var dto = new UpdateInvoiceDto("", null, null, null);
        var existingInvoice = new Invoice { Id = 1, InvoiceNumber = "INV-001" };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingInvoice);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new UpdateInvoiceDto(null, -50.00m, null, null);
        var existingInvoice = new Invoice { Id = 1, InvoiceNumber = "INV-001", Amount = 100.00m };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingInvoice);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidStatus_ThrowsArgumentException()
    {
        // Arrange
        var dto = new UpdateInvoiceDto(null, null, null, "InvalidStatus");
        var existingInvoice = new Invoice { Id = 1, InvoiceNumber = "INV-001" };

        _invoiceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingInvoice);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(1, dto));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        _invoiceRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _invoiceRepoMock.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(0));
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new InvoiceService(null!));
    }
}
