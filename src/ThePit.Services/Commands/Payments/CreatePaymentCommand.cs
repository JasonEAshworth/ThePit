using MediatR;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;

namespace ThePit.Services.Commands.Payments;

public record CreatePaymentCommand(
    int InvoiceId,
    decimal Amount,
    string PaymentMethod
) : IRequest<PaymentDto>;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IInvoiceRepository invoiceRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
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

        var payment = new Payment
        {
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentDate = DateTime.UtcNow,
            TransactionId = GenerateTransactionId(),
            Status = "Pending"
        };

        var created = await _paymentRepository.CreateAsync(payment);

        return new PaymentDto
        {
            Id = created.Id,
            InvoiceId = created.InvoiceId,
            Amount = created.Amount,
            PaymentDate = created.PaymentDate,
            PaymentMethod = created.PaymentMethod,
            TransactionId = created.TransactionId,
            Status = created.Status
        };
    }

    private static string GenerateTransactionId()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
    }
}
