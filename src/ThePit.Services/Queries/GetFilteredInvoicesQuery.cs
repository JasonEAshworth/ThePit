using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Queries;

public record GetFilteredInvoicesQuery(
    string? Status = null,
    string? CustomerEmail = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null) : IRequest<IEnumerable<InvoiceDto>>;

public class GetFilteredInvoicesQueryHandler : IRequestHandler<GetFilteredInvoicesQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetFilteredInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetFilteredInvoicesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<ThePit.DataAccess.Entities.Invoice> invoices;

        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            invoices = await _invoiceRepository.GetByCustomerEmailAsync(request.CustomerEmail);
        }
        else
        {
            invoices = await _invoiceRepository.GetAllAsync();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            invoices = invoices.Where(i => i.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (request.DueDateFrom.HasValue)
        {
            invoices = invoices.Where(i => i.DueDate >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            invoices = invoices.Where(i => i.DueDate <= request.DueDateTo.Value);
        }

        if (request.MinAmount.HasValue)
        {
            invoices = invoices.Where(i => i.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            invoices = invoices.Where(i => i.Amount <= request.MaxAmount.Value);
        }

        return invoices.Select(invoice => new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Amount,
            invoice.DueDate,
            invoice.Status,
            invoice.CreatedAt));
    }
}
