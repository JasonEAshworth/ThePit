using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Services;

[Obsolete("PaymentService is deprecated. Use CQRS commands and queries instead.")]
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public PaymentService(IPaymentRepository paymentRepository, IInvoiceRepository invoiceRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    }

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Payment ID must be greater than zero", nameof(id));

        var payment = await _paymentRepository.GetByIdAsync(id);
        return payment == null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto?> GetByTransactionIdAsync(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be null or empty", nameof(transactionId));

        var payment = await _paymentRepository.GetByTransactionIdAsync(transactionId);
        return payment == null ? null : MapToDto(payment);
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        var payments = await _paymentRepository.GetAllAsync();
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByInvoiceIdAsync(int invoiceId)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(invoiceId));

        var payments = await _paymentRepository.GetByInvoiceIdAsync(invoiceId);
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetFilteredAsync(string? status = null, string? paymentMethod = null)
    {
        var payments = await _paymentRepository.GetFilteredAsync(status, paymentMethod);
        return payments.Select(MapToDto);
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto)
    {
        ValidateCreateDto(dto);

        var invoice = await _invoiceRepository.GetByIdAsync(dto.InvoiceId);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {dto.InvoiceId} not found");

        var payment = new Payment
        {
            InvoiceId = dto.InvoiceId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            PaymentDate = DateTime.UtcNow,
            TransactionId = GenerateTransactionId(),
            Status = "Pending"
        };

        var created = await _paymentRepository.CreateAsync(payment);
        return MapToDto(created);
    }

    public async Task<PaymentDto> UpdateStatusAsync(UpdatePaymentDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.Id <= 0)
            throw new ArgumentException("Payment ID must be greater than zero", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Status))
            throw new ArgumentException("Status cannot be null or empty", nameof(dto));

        var validStatuses = new[] { "Pending", "Processing", "Completed", "Failed", "Refunded" };
        if (!validStatuses.Contains(dto.Status))
            throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}", nameof(dto));

        var payment = await _paymentRepository.GetByIdAsync(dto.Id);
        if (payment == null)
            throw new InvalidOperationException($"Payment with ID {dto.Id} not found");

        payment.Status = dto.Status;
        var updated = await _paymentRepository.UpdateAsync(payment);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Payment ID must be greater than zero", nameof(id));

        return await _paymentRepository.DeleteAsync(id);
    }

    public async Task<PaymentDto> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod)
    {
        if (invoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(invoiceId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method cannot be null or empty", nameof(paymentMethod));

        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {invoiceId} not found");

        if (invoice.Status == "Paid")
            throw new InvalidOperationException($"Invoice {invoiceId} has already been paid");

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            PaymentDate = DateTime.UtcNow,
            TransactionId = GenerateTransactionId(),
            Status = "Processing"
        };

        var created = await _paymentRepository.CreateAsync(payment);

        // Simulate payment processing
        created.Status = "Completed";
        var processed = await _paymentRepository.UpdateAsync(created);

        // Update invoice status
        invoice.Status = "Paid";
        invoice.PaidAt = DateTime.UtcNow;
        await _invoiceRepository.UpdateAsync(invoice);

        return MapToDto(processed);
    }

    private static void ValidateCreateDto(CreatePaymentDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.InvoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(dto));

        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
            throw new ArgumentException("Payment method cannot be null or empty", nameof(dto));
    }

    private static string GenerateTransactionId()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            TransactionId = payment.TransactionId,
            Status = payment.Status
        };
    }
}
