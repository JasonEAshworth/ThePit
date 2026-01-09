using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Queries;

public record GetPaymentByIdQuery(int Id) : IRequest<PaymentDto?>;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public GetPaymentByIdQueryHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetByIdAsync(request.Id);
    }
}
