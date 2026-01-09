using MediatR;
using ThePit.DataAccess.Entities;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Commands.Invoices;

public record CreateInvoiceCommand(
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDate
) : IRequest<InvoiceDto>;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public CreateInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
            throw new ArgumentException("Invoice number cannot be null or empty");

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero");

        var invoice = new Invoice
        {
            InvoiceNumber = request.InvoiceNumber,
            Amount = request.Amount,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        var created = await _invoiceRepository.CreateAsync(invoice);

        return new InvoiceDto(
            created.Id,
            created.InvoiceNumber,
            created.Amount,
            created.DueDate,
            created.Status,
            created.CreatedAt);
    }
}
