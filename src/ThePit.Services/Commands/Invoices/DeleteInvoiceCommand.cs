using MediatR;
using ThePit.DataAccess.Interfaces;

namespace ThePit.Services.Commands.Invoices;

public record DeleteInvoiceCommand(int Id) : IRequest<bool>;

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public DeleteInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
    }

    public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        return await _invoiceRepository.DeleteAsync(request.Id);
    }
}
