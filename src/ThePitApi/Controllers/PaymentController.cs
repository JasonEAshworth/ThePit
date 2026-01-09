using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThePit.Services.Commands.Payments;
using ThePit.Services.DTOs;
using ThePit.Services.Queries.Payments;

namespace ThePitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var payments = await _mediator.Send(new GetAllPaymentsQuery(), cancellationToken);
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        if (payment is null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<ActionResult<PaymentDto>> GetByTransactionId(string transactionId, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await _mediator.Send(new GetPaymentByTransactionIdQuery(transactionId), cancellationToken);
            if (payment is null)
                return NotFound();

            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("invoice/{invoiceId:int}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByInvoiceId(int invoiceId, CancellationToken cancellationToken)
    {
        var payments = await _mediator.Send(new GetPaymentsByInvoiceQuery(invoiceId), cancellationToken);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreatePaymentCommand(dto.InvoiceId, dto.Amount, dto.PaymentMethod);
            var payment = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ProcessPaymentCommand(request.InvoiceId, request.Amount, request.PaymentMethod);
            var payment = await _mediator.Send(command, cancellationToken);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<PaymentDto>> UpdateStatus(int id, [FromBody] UpdatePaymentStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdatePaymentCommand(id, request.Status);
            var payment = await _mediator.Send(command, cancellationToken);
            return Ok(payment);
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
        var deleted = await _mediator.Send(new DeletePaymentCommand(id), cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

public record ProcessPaymentRequest(int InvoiceId, decimal Amount, string PaymentMethod);
public record UpdatePaymentStatusRequest(string Status);
