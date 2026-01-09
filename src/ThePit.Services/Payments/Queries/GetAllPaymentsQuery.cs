using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Queries;

public record GetAllPaymentsQuery : IRequest<IEnumerable<PaymentDto>>;

public class GetAllPaymentsQueryHandler : IRequestHandler<GetAllPaymentsQuery, IEnumerable<PaymentDto>>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public GetAllPaymentsQueryHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IEnumerable<PaymentDto>> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetAllAsync();
    }
}
