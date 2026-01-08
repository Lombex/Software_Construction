using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CSharpAPI.Controllers
{
    [Route("api/billing")]
    [ApiController]
    [Authorize] // All billing endpoints require authentication
    public class C_Billing : ControllerBase
    {
        private readonly IBillingService _billingService;

        public C_Billing(IBillingService billingService)
        {
            _billingService = billingService;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(idClaim, out var id) ? id : null;
            }
        }

        private bool IsAdminOrAbove => User.IsInRole("SuperAdmin") || User.IsInRole("ParkingLotAdmin");

        // GET /api/billing/all - Get all billing records (admin only)
        [HttpGet("all")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetAll()
        {
            var bills = await _billingService.GetAll();
            return Ok(bills);
        }

        // GET /api/billing/mine - Get current user's billing records
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            if (CurrentUserId == null) return Unauthorized();
            
            var bills = await _billingService.GetForUser(CurrentUserId.Value);
            return Ok(bills);
        }

        // GET /api/billing/mine/pending - Get current user's pending bills
        [HttpGet("mine/pending")]
        public async Task<IActionResult> GetMinePending()
        {
            if (CurrentUserId == null) return Unauthorized();
            
            var bills = await _billingService.GetPendingForUser(CurrentUserId.Value);
            return Ok(bills);
        }

        // GET /api/billing/mine/overdue - Get current user's overdue bills
        [HttpGet("mine/overdue")]
        public async Task<IActionResult> GetMineOverdue()
        {
            if (CurrentUserId == null) return Unauthorized();
            
            var bills = await _billingService.GetOverdueForUser(CurrentUserId.Value);
            return Ok(bills);
        }

        // GET /api/billing/user/{userId} - Get billing records for a user (admin only)
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest("Invalid user ID.");
            
            var bills = await _billingService.GetForUser(userId);
            return Ok(bills);
        }

        // GET /api/billing/{id} - Get billing record by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid billing ID.");
            
            var bill = await _billingService.GetById(id);
            if (bill == null) return NotFound("Billing record not found.");

            // Users can only view their own bills, admins can view any
            if (!IsAdminOrAbove && (CurrentUserId == null || bill.user_id != CurrentUserId.Value))
                return Forbid();

            return Ok(bill);
        }

        // POST /api/billing/create - Create a new billing record (admin only)
        [HttpPost("create")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Create([FromBody] M_Billing bill)
        {
            if (bill == null) return BadRequest("Billing data is required.");
            if (bill.user_id == Guid.Empty) return BadRequest("User ID is required.");
            if (bill.amount <= 0) return BadRequest("Amount must be greater than zero.");

            try
            {
                var created = await _billingService.Create(bill);
                return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the billing record.", error = ex.Message });
            }
        }

        // PUT /api/billing/update/{id} - Update a billing record
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] M_Billing bill)
        {
            if (id == Guid.Empty) return BadRequest("Invalid billing ID.");
            if (bill == null) return BadRequest("Billing data is required.");

            var existing = await _billingService.GetById(id);
            if (existing == null) return NotFound("Billing record not found.");

            // Users can only update description of their own bills, admins can update all fields
            if (!IsAdminOrAbove)
            {
                if (CurrentUserId == null || existing.user_id != CurrentUserId.Value)
                    return Forbid();

                // Users can only update description
                existing.description = bill.description;
            }
            else
            {
                // Admins can update all fields except user_id and paid status
                existing.amount = bill.amount;
                existing.currency = bill.currency;
                existing.description = bill.description;
                existing.due_date = bill.due_date;
                existing.status = bill.status;
            }

            var updated = await _billingService.Update(existing);
            if (!updated) return StatusCode(500, "Failed to update billing record.");

            return NoContent();
        }

        // POST /api/billing/{id}/mark-paid - Mark a bill as paid
        [HttpPost("{id}/mark-paid")]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid billing ID.");

            var bill = await _billingService.GetById(id);
            if (bill == null) return NotFound("Billing record not found.");

            // Users can mark their own bills as paid, admins can mark any
            if (!IsAdminOrAbove && (CurrentUserId == null || bill.user_id != CurrentUserId.Value))
                return Forbid();

            var updated = await _billingService.MarkPaid(id);
            if (!updated) return StatusCode(500, "Failed to mark bill as paid.");

            return Ok(new { message = "Bill marked as paid." });
        }

        // POST /api/billing/{id}/cancel - Cancel a bill (admin only)
        [HttpPost("{id}/cancel")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid billing ID.");

            try
            {
                var cancelled = await _billingService.Cancel(id);
                if (!cancelled) return NotFound("Billing record not found.");

                return Ok(new { message = "Bill cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE /api/billing/delete/{id} - Delete a billing record (SuperAdmin only)
        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Invalid billing ID.");

            var deleted = await _billingService.Delete(id);
            if (!deleted) return NotFound("Billing record not found.");

            return NoContent();
        }

        // GET /api/billing/monthly-bundle/{userId} - Get monthly bundle invoices for a user (admin only)
        [HttpGet("monthly-bundle/{userId}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetMonthlyBundles(Guid userId, [FromQuery] DateTime? month = null)
        {
            if (userId == Guid.Empty) return BadRequest("Invalid user ID.");
            
            var targetMonth = month ?? DateTime.UtcNow;
            var bundles = await _billingService.GetMonthlyBundlesForUser(userId, targetMonth);
            
            return Ok(bundles);
        }
    }
}

