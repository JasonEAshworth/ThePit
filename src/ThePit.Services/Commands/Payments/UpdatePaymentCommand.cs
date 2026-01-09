using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.DTOs;

namespace ThePit.Services.Commands.Payments;

public record UpdatePaymentCommand(
    int Id,
    string Status
) : IRequest<PaymentDto>;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, PaymentDto>
{
    private static readonly string[] ValidStatuses = { "Pending", "Processing", "Completed", "Failed", "Refunded" };

    private readonly IPaymentRepository _paymentRepository;

    public UpdatePaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
    }

    public async Task<PaymentDto> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            throw new ArgumentException("Payment ID must be greater than zero");

        if (string.IsNullOrWhiteSpace(request.Status))
            throw new ArgumentException("Status cannot be null or empty");

        if (!ValidStatuses.Contains(request.Status))
            throw new ArgumentException($"Invalid status. Must be one of: {string.Join(", ", ValidStatuses)}");

        var payment = await _paymentRepository.GetByIdAsync(request.Id);
        if (payment == null)
            throw new InvalidOperationException($"Payment with ID {request.Id} not found");

        payment.Status = request.Status;
        var updated = await _paymentRepository.UpdateAsync(payment);

        return new PaymentDto
        {
            Id = updated.Id,
            InvoiceId = updated.InvoiceId,
            Amount = updated.Amount,
            PaymentDate = updated.PaymentDate,
            PaymentMethod = updated.PaymentMethod,
            TransactionId = updated.TransactionId,
            Status = updated.Status
        };
    }
}
