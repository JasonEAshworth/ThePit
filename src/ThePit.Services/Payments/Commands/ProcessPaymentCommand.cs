using MediatR;
using ThePit.Services.DTOs;

namespace ThePit.Services.Payments.Commands;

public record ProcessPaymentCommand(int InvoiceId, decimal Amount, string PaymentMethod) : IRequest<PaymentDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentDto>
{
    private readonly Interfaces.IPaymentService _paymentService;

    public ProcessPaymentCommandHandler(Interfaces.IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.ProcessPaymentAsync(request.InvoiceId, request.Amount, request.PaymentMethod);
    }
}
