using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThePit.Services.DTOs;
using ThePit.Services.Payments.Commands;
using ThePit.Services.Payments.Queries;

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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var payments = await _mediator.Send(new GetAllPaymentsQuery());
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id));
        if (payment is null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<ActionResult<PaymentDto>> GetByTransactionId(string transactionId)
    {
        try
        {
            var payment = await _mediator.Send(new GetPaymentByTransactionIdQuery(transactionId));
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
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByInvoiceId(int invoiceId)
    {
        var payments = await _mediator.Send(new GetPaymentsByInvoiceIdQuery(invoiceId));
        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var payment = await _mediator.Send(new CreatePaymentCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var payment = await _mediator.Send(new ProcessPaymentCommand(
                request.InvoiceId,
                request.Amount,
                request.PaymentMethod));
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
    public async Task<ActionResult<PaymentDto>> UpdateStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
    {
        try
        {
            var payment = await _mediator.Send(new UpdatePaymentStatusCommand(id, request.Status));
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
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _mediator.Send(new DeletePaymentCommand(id));
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

public record ProcessPaymentRequest(int InvoiceId, decimal Amount, string PaymentMethod);
public record UpdatePaymentStatusRequest(string Status);
