using MediatR;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Commands.Invoice;

public record CreateInvoiceCommand(
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDate) : IRequest<InvoiceDto>;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _repository;

    public CreateInvoiceCommandHandler(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var invoice = new DataAccess.Entities.Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            Amount = request.Amount,
            DueDate = request.DueDate,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(invoice);

        return new InvoiceDto(
            created.Id,
            created.InvoiceNumber,
            created.Amount,
            created.DueDate,
            created.Status,
            created.CreatedAt);
    }

    private static void ValidateRequest(CreateInvoiceCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
            throw new ArgumentException("Invoice number is required");

        if (request.InvoiceNumber.Length > 50)
            throw new ArgumentException("Invoice number cannot exceed 50 characters");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        if (request.Amount > 999999999.99m)
            throw new ArgumentException("Amount exceeds maximum allowed value");

        if (request.DueDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Due date cannot be in the past");
    }
}
