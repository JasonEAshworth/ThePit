using ThePit.Services.DTOs;

namespace ThePit.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto?> GetByIdAsync(int id);
    Task<PaymentDto?> GetByTransactionIdAsync(string transactionId);
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<IEnumerable<PaymentDto>> GetByInvoiceIdAsync(int invoiceId);
    Task<IEnumerable<PaymentDto>> GetFilteredAsync(string? status = null, string? paymentMethod = null);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
    Task<PaymentDto> UpdateStatusAsync(UpdatePaymentDto dto);
    Task<bool> DeleteAsync(int id);
    Task<PaymentDto> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod);
}
