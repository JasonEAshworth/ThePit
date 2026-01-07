using Microsoft.AspNetCore.Mvc;
using ThePit.Services.Interfaces;

namespace ThePitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
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
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _invoiceService.CreateAsync(dto, cancellationToken);
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
            var invoice = await _invoiceService.UpdateAsync(id, dto, cancellationToken);
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
        var deleted = await _invoiceService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
