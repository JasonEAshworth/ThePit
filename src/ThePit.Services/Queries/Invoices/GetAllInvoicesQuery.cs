using MediatR;
using ThePit.DataAccess.Interfaces;
using ThePit.Services.Interfaces;

namespace ThePit.Services.Queries.Invoices;

public record GetAllInvoicesQuery : IRequest<IEnumerable<InvoiceDto>>;

public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, IEnumerable<InvoiceDto>>
{
    private readonly IInvoiceRepository _repository;

    public GetAllInvoicesQueryHandler(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _repository.GetAllAsync();

        return invoices.Select(invoice => new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.Amount,
            invoice.DueDate,
            invoice.Status,
            invoice.CreatedAt));
    }
}
