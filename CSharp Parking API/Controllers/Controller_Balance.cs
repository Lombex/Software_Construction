using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v2/balance")]
    [Authorize]
    public class C_Balance : ControllerBase
    {
        private readonly IUserBalanceService _balanceService;
        private readonly ILogger<C_Balance> _logger;

        public C_Balance(IUserBalanceService balanceService, ILogger<C_Balance> logger)
        {
            _balanceService = balanceService;
            _logger = logger;
        }

        // Get current user's balance
        [HttpGet("me")]
        public async Task<IActionResult> GetMyBalance()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var balance = await _balanceService.GetBalanceForUser(userId.Value);
                if (balance == null)
                {
                    // Create balance if it doesn't exist
                    balance = await _balanceService.CreateBalance(userId.Value, 0);
                }
                return Ok(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance for user {UserId}", userId);
                return StatusCode(500, "Error retrieving balance.");
            }
        }

        // Get balance for a specific user (admin only)
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetUserBalance(Guid userId)
        {
            try
            {
                var balance = await _balanceService.GetBalanceForUser(userId);
                if (balance == null)
                    return NotFound("Balance not found for this user.");
                return Ok(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting balance for user {UserId}", userId);
                return StatusCode(500, "Error retrieving balance.");
            }
        }

        // Add money to current user's balance
        [HttpPost("me/add")]
        public async Task<IActionResult> AddToMyBalance([FromBody] AddBalanceRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            try
            {
                var balance = await _balanceService.AddToBalance(userId.Value, request.Amount, request.Description);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to balance for user {UserId}", userId);
                return StatusCode(500, "Error adding to balance.");
            }
        }

        // Get transaction history for current user
        [HttpGet("me/transactions")]
        public async Task<IActionResult> GetMyTransactions([FromQuery] int? limit = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var transactions = await _balanceService.GetTransactionHistory(userId.Value, limit);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for user {UserId}", userId);
                return StatusCode(500, "Error retrieving transactions.");
            }
        }

        // Check if user has sufficient balance (for NFC payment)
        [HttpGet("me/check/{amount}")]
        public async Task<IActionResult> CheckBalance(decimal amount)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var hasBalance = await _balanceService.HasSufficientBalance(userId.Value, amount);
                var currentBalance = await _balanceService.GetBalanceAmount(userId.Value);
                return Ok(new { hasSufficientBalance = hasBalance, currentBalance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking balance for user {UserId}", userId);
                return StatusCode(500, "Error checking balance.");
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null && Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public class AddBalanceRequest
    {
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}

