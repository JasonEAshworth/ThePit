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
    private readonly IInvoiceRepository _invoiceRepository;

    public UpdateInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    }

    public async Task<InvoiceDto> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id);
        if (invoice == null)
            throw new InvalidOperationException($"Invoice with ID {request.Id} not found");

        if (request.InvoiceNumber != null)
        {
            if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
                throw new ArgumentException("Invoice number cannot be empty");
            invoice.InvoiceNumber = request.InvoiceNumber;
        }

        if (request.Amount.HasValue)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");
            invoice.Amount = request.Amount.Value;
        }

        if (request.DueDate.HasValue)
            invoice.DueDate = request.DueDate.Value;

        if (request.Status != null)
            invoice.Status = request.Status;

        var updated = await _invoiceRepository.UpdateAsync(invoice);

        return new InvoiceDto(
            updated.Id,
            updated.InvoiceNumber,
            updated.Amount,
            updated.DueDate,
            updated.Status,
            updated.CreatedAt);
    }
}
