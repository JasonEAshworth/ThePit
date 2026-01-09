using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThePit.Services.Commands.Invoice;
using ThePit.Services.Interfaces;

namespace ThePitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IMediator _mediator;

    public InvoiceController(IInvoiceService invoiceService, IMediator mediator)
    {
        _invoiceService = invoiceService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await _invoiceService.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
            return NotFound();

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> Update(int id, [FromBody] UpdateInvoiceDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateInvoiceCommand(id, dto.InvoiceNumber, dto.Amount, dto.DueDate, dto.Status);
            var invoice = await _mediator.Send(command, cancellationToken);
            return Ok(invoice);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteInvoiceCommand(id);
        var deleted = await _mediator.Send(command, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
