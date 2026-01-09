using Microsoft.AspNetCore.Mvc;
using ThePit.Services.DTOs;
using ThePit.Services.Interfaces;

namespace ThePitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments(
        [FromQuery] string? status = null,
        [FromQuery] string? method = null)
    {
        var payments = await _paymentService.GetFilteredAsync(status, method);
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("by-invoice/{invoiceId:int}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByInvoice(int invoiceId)
    {
        var payments = await _paymentService.GetByInvoiceIdAsync(invoiceId);
        return Ok(payments);
    }
}
