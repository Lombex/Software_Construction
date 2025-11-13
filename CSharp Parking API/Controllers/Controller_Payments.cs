using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAPI.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class C_Payments : ControllerBase
    {
        private readonly IPaymentsService PaymentsService;
        public C_Payments(IPaymentsService paymentsService)
        {
            PaymentsService = paymentsService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPayments([FromQuery] int page)
        {
            var payments = await PaymentsService.GetAllPayments();

            int totalItem = payments.Count;
            int totalPages = (int)Math.Ceiling(totalItem / (double)10);
            if (page > totalPages) return BadRequest("Page number exceeds total pages.");

            var elements = payments.Skip((page * 10)).Take(10).Select(x => new
            {
                id = x.id,
                session_id = x.session_id,
                transactions = x.transactions,
                initiator = x.initiator,
                amount = x.amount,
                created_at = x.created_at,
                completed = x.completed,
                hash = x.hash,
                t_data = x.t_data,
                parking_lot_id = x.parking_lot_id
            });

            var response = new
            {
                Page = page,
                PageSize = 10,
                TotalItems = totalItem,
                totalPages = totalPages,
                Payments = elements
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentByID(Guid Id)
        {
            var payment = await PaymentsService.getByID(Id);
            if (payment == null) return NotFound($"Payment with id {Id} not found.");
            return Ok(payment);
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] M_Payments newPayment)
        {
            await PaymentsService.CreatePayment(newPayment);
            return Ok("Payment created successfully.");
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] M_Payments updatedPayment)
        {
            await PaymentsService.UpdatePayment(id, updatedPayment);
            return Ok("Payment updated successfully.");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            await PaymentsService.DeletePayment(id);
            return Ok("Payment deleted successfully.");
        }
    }
}