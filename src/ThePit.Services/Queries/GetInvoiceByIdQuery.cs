using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Queries;

public record GetInvoiceByIdQuery(int Id) : IRequest<InvoiceDto?>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.Id);

        if (invoice is null)
            return null;

        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Amount,
            invoice.DueDate,
            invoice.Status,
            invoice.CreatedAt);
    }
}
