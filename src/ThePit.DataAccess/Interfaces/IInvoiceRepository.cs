using ThePit.DataAccess.Entities;

namespace ThePit.DataAccess.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<IEnumerable<Invoice>> GetByCustomerEmailAsync(string email);
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<Invoice> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(int id);
}
