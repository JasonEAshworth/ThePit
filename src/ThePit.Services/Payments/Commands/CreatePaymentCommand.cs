using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Commands;

public record CreatePaymentCommand(CreatePaymentDto Dto) : IRequest<PaymentDto>;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public CreatePaymentCommandHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.CreateAsync(request.Dto);
    }
}
