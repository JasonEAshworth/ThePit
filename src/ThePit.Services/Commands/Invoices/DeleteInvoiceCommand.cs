using MediatR;
using ThePit.DataAccess.Interfaces;

namespace ThePit.Services.Commands.Invoices;

public record DeleteInvoiceCommand(int Id) : IRequest<bool>;

public class DeleteInvoiceCommandHandler : IRequestHandler<DeleteInvoiceCommand, bool>
{
    private readonly IInvoiceRepository _repository;

    public DeleteInvoiceCommandHandler(IInvoiceRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<bool> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            throw new ArgumentException("Invoice ID must be positive");

        return await _repository.DeleteAsync(request.Id);
    }
}
