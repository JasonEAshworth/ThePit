using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Services;

[Obsolete("InvoiceService is deprecated. Use CQRS commands and queries instead.")]
public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _repository;

    public InvoiceService(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Invoice ID must be positive", nameof(id));

        var invoice = await _repository.GetByIdAsync(id);
        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _repository.GetAllAsync();
        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        ValidateCreateDto(dto);

        var existing = await _repository.GetByInvoiceNumberAsync(dto.InvoiceNumber);
        if (existing is not null)
            throw new InvalidOperationException($"Invoice with number '{dto.InvoiceNumber}' already exists");

        var invoice = new Invoice
        {
            InvoiceNumber = dto.InvoiceNumber,
            Amount = dto.Amount,
            DueDate = dto.DueDate,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(invoice);
        return MapToDto(created);
    }

    public async Task<InvoiceDto> UpdateAsync(int id, UpdateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Invoice ID must be positive", nameof(id));

        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            throw new InvalidOperationException($"Invoice with ID {id} not found");

        if (dto.InvoiceNumber is not null)
        {
            ValidateInvoiceNumber(dto.InvoiceNumber);
            var duplicate = await _repository.GetByInvoiceNumberAsync(dto.InvoiceNumber);
            if (duplicate is not null && duplicate.Id != id)
                throw new InvalidOperationException($"Invoice with number '{dto.InvoiceNumber}' already exists");
            existing.InvoiceNumber = dto.InvoiceNumber;
        }

        if (dto.Amount.HasValue)
        {
            ValidateAmount(dto.Amount.Value);
            existing.Amount = dto.Amount.Value;
        }

        if (dto.DueDate.HasValue)
        {
            existing.DueDate = dto.DueDate.Value;
        }

        if (dto.Status is not null)
        {
            ValidateStatus(dto.Status);
            existing.Status = dto.Status;

            if (dto.Status == "Paid" && existing.PaidAt is null)
            {
                existing.PaidAt = DateTime.UtcNow;
            }
        }

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            throw new ArgumentException("Invoice ID must be positive", nameof(id));

        return await _repository.DeleteAsync(id);
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Amount,
            invoice.DueDate,
            invoice.Status,
            invoice.CreatedAt);
    }

    private static void ValidateCreateDto(CreateInvoiceDto dto)
    {
        ValidateInvoiceNumber(dto.InvoiceNumber);
        ValidateAmount(dto.Amount);

        if (dto.DueDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Due date cannot be in the past", nameof(dto));
    }

    private static void ValidateInvoiceNumber(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required");

        if (invoiceNumber.Length > 50)
            throw new ArgumentException("Invoice number cannot exceed 50 characters");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        if (amount > 999999999.99m)
            throw new ArgumentException("Amount exceeds maximum allowed value");
    }

    private static void ValidateStatus(string status)
    {
        var validStatuses = new[] { "Pending", "Paid", "Overdue", "Cancelled" };
        if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Status must be one of: {string.Join(", ", validStatuses)}");
    }
}
