using MediatR;
using ThePit.DataAccess.Interfaces;

namespace ThePit.Services.Commands.Payments;

public record DeletePaymentCommand(int Id) : IRequest<bool>;

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, bool>
{
    private readonly IPaymentRepository _paymentRepository;

    public DeletePaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    public async Task<bool> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        return await _paymentRepository.DeleteAsync(request.Id);
    }
}
