using ThePit.DataAccess.Entities;

namespace ThePit.DataAccess.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetByTransactionIdAsync(string transactionId);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId);
    Task<IEnumerable<Payment>> GetFilteredAsync(string? status = null, string? paymentMethod = null);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
    Task<bool> DeleteAsync(int id);
}
