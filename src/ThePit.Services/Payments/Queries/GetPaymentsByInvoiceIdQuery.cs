using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Queries;

public record GetPaymentsByInvoiceIdQuery(int InvoiceId) : IRequest<IEnumerable<PaymentDto>>;

public class GetPaymentsByInvoiceIdQueryHandler : IRequestHandler<GetPaymentsByInvoiceIdQuery, IEnumerable<PaymentDto>>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public GetPaymentsByInvoiceIdQueryHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByInvoiceIdQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetByInvoiceIdAsync(request.InvoiceId);
    }
}
