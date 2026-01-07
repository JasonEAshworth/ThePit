using Microsoft.EntityFrameworkCore;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;

namespace ThePit.DataAccess.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ThePitDbContext _context;

    public InvoiceRepository(ThePitDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number cannot be null or empty", nameof(invoiceNumber));

        return await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        return await _context.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerEmail == email)
            .ToListAsync();
    }

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        invoice.CreatedAt = DateTime.UtcNow;
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice> UpdateAsync(Invoice invoice)
    {
        if (invoice == null)
            throw new ArgumentNullException(nameof(invoice));

        var existing = await _context.Invoices.FindAsync(invoice.Id);
        if (existing == null)
            throw new InvalidOperationException($"Invoice with Id {invoice.Id} not found");

        _context.Entry(existing).CurrentValues.SetValues(invoice);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return false;

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        return true;
    }
}
