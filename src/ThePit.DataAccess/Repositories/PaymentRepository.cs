using Microsoft.EntityFrameworkCore;
using ThePit.DataAccess.Data;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;

namespace ThePit.DataAccess.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ThePitDbContext _context;

    public PaymentRepository(ThePitDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be null or empty", nameof(transactionId));

        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _context.Payments
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoiceId)
            .ToListAsync();
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        var existing = await _context.Payments.FindAsync(payment.Id);
        if (existing == null)
            throw new InvalidOperationException($"Payment with ID {payment.Id} not found");

        _context.Entry(existing).CurrentValues.SetValues(payment);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return false;

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }
}
