namespace ThePit.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceDto> CreateAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<InvoiceDto> UpdateAsync(int id, UpdateInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public record InvoiceDto(
    int Id,
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDate,
    string Status,
    DateTime CreatedAt);

public record CreateInvoiceDto(
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDate);

public record UpdateInvoiceDto(
    string? InvoiceNumber,
    decimal? Amount,
    DateTime? DueDate,
    string? Status);
