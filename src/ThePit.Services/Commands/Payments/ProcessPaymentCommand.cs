using MediatR;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;

namespace ThePit.Services.Commands.Payments;

public record ProcessPaymentCommand(
    int InvoiceId,
    decimal Amount,
    string PaymentMethod
) : IRequest<PaymentDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.InvoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            throw new ArgumentException("Payment method cannot be null or empty");

        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found");

        if (invoice.Status == "Paid")
            throw new InvalidOperationException($"Invoice {request.InvoiceId} has already been paid");

        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentDate = DateTime.UtcNow,
            TransactionId = GenerateTransactionId(),
            Status = "Processing"
        };

        var created = await _paymentRepository.CreateAsync(payment);

        // Simulate payment processing - in production this would integrate with payment gateway
        created.Status = "Completed";
        var processed = await _paymentRepository.UpdateAsync(created);

        // Update invoice status
        invoice.Status = "Paid";
        invoice.PaidAt = DateTime.UtcNow;
        await _invoiceRepository.UpdateAsync(invoice);

        return new PaymentDto
        {
            Id = processed.Id,
            InvoiceId = processed.InvoiceId,
            Amount = processed.Amount,
            PaymentDate = processed.PaymentDate,
            PaymentMethod = processed.PaymentMethod,
            TransactionId = processed.TransactionId,
            Status = processed.Status
        };
    }

    private static string GenerateTransactionId()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
    }
}
