using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Queries.Invoices;

public record GetInvoiceByIdQuery(int Id) : IRequest<InvoiceDto?>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _repository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            throw new ArgumentException("Invoice ID must be positive");

        var invoice = await _repository.GetByIdAsync(request.Id);
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
