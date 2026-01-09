using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;

namespace ThePit.Services.Queries.Payments;

public record GetPaymentByTransactionIdQuery(string TransactionId) : IRequest<PaymentDto?>;

public class GetPaymentByTransactionIdQueryHandler : IRequestHandler<GetPaymentByTransactionIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByTransactionIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    public async Task<PaymentDto?> Handle(GetPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId))
            throw new ArgumentException("Transaction ID cannot be null or empty");

        var payment = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId);

        if (payment is null)
            return null;

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
