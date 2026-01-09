using MediatR;

namespace ThePit.Services.Payments.Commands;

public record DeletePaymentCommand(int Id) : IRequest<bool>;

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, bool>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public DeletePaymentCommandHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<bool> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.DeleteAsync(request.Id);
    }
}
