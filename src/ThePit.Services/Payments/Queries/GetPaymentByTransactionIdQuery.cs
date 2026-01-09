using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Queries;

public record GetPaymentByTransactionIdQuery(string TransactionId) : IRequest<PaymentDto?>;

public class GetPaymentByTransactionIdQueryHandler : IRequestHandler<GetPaymentByTransactionIdQuery, PaymentDto?>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public GetPaymentByTransactionIdQueryHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetByTransactionIdAsync(request.TransactionId);
    }
}
