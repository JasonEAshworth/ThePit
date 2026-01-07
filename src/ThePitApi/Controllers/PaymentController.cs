using Microsoft.AspNetCore.Mvc;
using ThePit.Services.DTOs;
using ThePit.Services.Interfaces;

namespace ThePitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var payments = await _paymentService.GetAllAsync();
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment is null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("transaction/{transactionId}")]
    public async Task<ActionResult<PaymentDto>> GetByTransactionId(string transactionId)
    {
        try
        {
            var payment = await _paymentService.GetByTransactionIdAsync(transactionId);
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
        var payments = await _paymentService.GetByInvoiceIdAsync(invoiceId);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.CreateAsync(dto);
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
            var payment = await _paymentService.ProcessPaymentAsync(
                request.InvoiceId,
                request.Amount,
                request.PaymentMethod);
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
            var dto = new UpdatePaymentDto { Id = id, Status = request.Status };
            var payment = await _paymentService.UpdateStatusAsync(dto);
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
        var deleted = await _paymentService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

public record ProcessPaymentRequest(int InvoiceId, decimal Amount, string PaymentMethod);
public record UpdatePaymentStatusRequest(string Status);
