namespace ThePit.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentDto>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto dto, CancellationToken cancellationToken = default);
    Task<bool> RefundAsync(int paymentId, CancellationToken cancellationToken = default);
}

public record PaymentDto(
    int Id,
    int InvoiceId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    DateTime ProcessedAt);

public record ProcessPaymentDto(
    int InvoiceId,
    decimal Amount,
    string PaymentMethod);
