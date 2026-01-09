using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Commands;

public record UpdatePaymentStatusCommand(int Id, string Status) : IRequest<PaymentDto>;

public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, PaymentDto>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public UpdatePaymentStatusCommandHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var dto = new UpdatePaymentDto { Id = request.Id, Status = request.Status };
        return await _paymentService.UpdateStatusAsync(dto);
    }
}
