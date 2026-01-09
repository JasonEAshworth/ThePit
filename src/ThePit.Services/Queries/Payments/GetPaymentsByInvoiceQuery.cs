using MediatR;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;

namespace ThePit.Services.Queries.Payments;

public record GetPaymentsByInvoiceQuery(int InvoiceId) : IRequest<IEnumerable<PaymentDto>>;

public class GetPaymentsByInvoiceQueryHandler : IRequestHandler<GetPaymentsByInvoiceQuery, IEnumerable<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsByInvoiceQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByInvoiceQuery request, CancellationToken cancellationToken)
    {
        if (request.InvoiceId <= 0)
        {
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(request));
        }

        var payments = await _paymentRepository.GetByInvoiceIdAsync(request.InvoiceId);
        return payments.Select(MapToDto);
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
