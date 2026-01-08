using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CSharpAPI.Database;
using static CSharpAPI.Models.M_Billing;

namespace CSharpAPI.Controllers
{
    [Route("api/payments")]
    [ApiController]
    [Authorize] // All payment endpoints require authentication
    public class C_Payments : ControllerBase
    {
        private readonly IPaymentsService PaymentsService;
        public C_Payments(IPaymentsService paymentsService)
        {
            PaymentsService = paymentsService;
        }

        [HttpGet("all")]
        [Authorize(Policy = "AdminOrAbove")] // ParkingLotAdmin or SuperAdmin can view all payments
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
        // Any authenticated user can create payment (for their own reservations)
        public async Task<IActionResult> CreatePayment([FromBody] M_Payments newPayment)
        {
            await PaymentsService.CreatePayment(newPayment);
            return Ok("Payment created successfully.");
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "AdminOrAbove")] // Only admins can update payments
        public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] M_Payments updatedPayment)
        {
            await PaymentsService.UpdatePayment(id, updatedPayment);
            return Ok("Payment updated successfully.");
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "SuperAdminOnly")] // Only SuperAdmin can delete payments
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            await PaymentsService.DeletePayment(id);
            return Ok("Payment deleted successfully.");
        }

        // POST /api/payments/{id}/refund - Refund a payment (admin only)
        [HttpPost("{id}/refund")]
        [Authorize(Policy = "AdminOrAbove")] // Only admins can refund payments
        public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundRequest request)
        {
            if (id == Guid.Empty) return BadRequest("Invalid payment ID.");
            if (string.IsNullOrWhiteSpace(request?.Reason)) 
                return BadRequest("Refund reason is required.");

            try
            {
                // Get current user ID from claims
                var adminUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminUserIdClaim) || !Guid.TryParse(adminUserIdClaim, out var adminUserId))
                    return Unauthorized("Invalid admin user ID.");

                var refundPayment = await PaymentsService.RefundPayment(id, request.Reason, adminUserId);

                // Create refund billing entry
                var billingService = HttpContext.RequestServices.GetRequiredService<IBillingService>();
                var dbContext = HttpContext.RequestServices.GetRequiredService<SQLite_Database>();
                
                // Get user_id from original payment's session or reservation
                var originalPayment = await PaymentsService.getByID(id);
                
                // Try to get user from session
                var session = await dbContext.Sessions.FirstOrDefaultAsync(s => s.id == originalPayment.session_id);
                Guid userId = Guid.Empty;
                
                if (session != null)
                {
                    // Get user from session's user field (username)
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.username == session.user);
                    if (user != null)
                        userId = user.id;
                }

                if (userId == Guid.Empty)
                    return BadRequest("Could not determine user for refund.");

                var refundBill = new M_Billing
                {
                    id = Guid.NewGuid(),
                    user_id = userId,
                    payment_id = refundPayment.id,
                    amount = (decimal)refundPayment.amount, // Convert float to decimal
                    currency = "EUR",
                    description = $"Refund for payment {id}: {request.Reason}",
                    due_date = DateTime.UtcNow, // Immediate
                    paid = true,
                    created_at = DateTime.UtcNow,
                    paid_at = DateTime.UtcNow,
                    type = BillingType.Refund,
                    status = BillingStatus.Paid
                };

                await billingService.Create(refundBill);

                return Ok(new { 
                    message = "Payment refunded successfully.", 
                    refundPayment = refundPayment,
                    refundBill = refundBill
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during refund.", error = ex.Message });
            }
        }

        // DTO for refund request
        public class RefundRequest
        {
            public string? Reason { get; set; }
        }
    }
}