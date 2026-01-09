using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThePit.Services.Commands.Invoices;
using ThePit.Services.Interfaces;
using ThePit.Services.Queries.Invoices;

namespace ThePitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await _mediator.Send(new GetAllInvoicesQuery(), cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var invoice = await _mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        if (invoice is null)
            return NotFound();

        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateInvoiceCommand(dto.InvoiceNumber, dto.Amount, dto.DueDate);
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
        var deleted = await _mediator.Send(new DeleteInvoiceCommand(id), cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
