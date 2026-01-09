using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Commands.Invoices;

public record UpdateInvoiceCommand(
    int Id,
    string? InvoiceNumber,
    decimal? Amount,
    DateTime? DueDate,
    string? Status
) : IRequest<InvoiceDto>;

public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, InvoiceDto>
{
    private static readonly string[] ValidStatuses = { "Pending", "Paid", "Overdue", "Cancelled" };

    private readonly IInvoiceRepository _repository;

    public UpdateInvoiceCommandHandler(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<InvoiceDto> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            throw new ArgumentException("Invoice ID must be positive");

        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing is null)
            throw new InvalidOperationException($"Invoice with ID {request.Id} not found");

        if (request.InvoiceNumber is not null)
        {
            ValidateInvoiceNumber(request.InvoiceNumber);
            existing.InvoiceNumber = request.InvoiceNumber;
        }

        if (request.Amount.HasValue)
        {
            ValidateAmount(request.Amount.Value);
            existing.Amount = request.Amount.Value;
        }

        if (request.DueDate.HasValue)
        {
            existing.DueDate = request.DueDate.Value;
        }

        if (request.Status is not null)
        {
            ValidateStatus(request.Status);
            existing.Status = request.Status;

            if (request.Status == "Paid" && existing.PaidAt is null)
            {
                existing.PaidAt = DateTime.UtcNow;
            }
        }

        var updated = await _repository.UpdateAsync(existing);

        return new InvoiceDto(
            updated.Id,
            updated.InvoiceNumber,
            updated.Amount,
            updated.DueDate,
            updated.Status,
            updated.CreatedAt);
    }

    private static void ValidateInvoiceNumber(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required");

        if (invoiceNumber.Length > 50)
            throw new ArgumentException("Invoice number cannot exceed 50 characters");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        if (amount > 999999999.99m)
            throw new ArgumentException("Amount exceeds maximum allowed value");
    }

    private static void ValidateStatus(string status)
    {
        if (!ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
