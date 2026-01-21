using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v2/nfc")]
    [AllowAnonymous] // NFC endpoints might be called by hardware
    public class C_NFC : ControllerBase
    {
        private readonly IUserBalanceService _balanceService;
        private readonly ISessionsService _sessionsService;
        private readonly IVehiclesService _vehiclesService;
        private readonly IParkinglotsService _parkinglotsService;
        private readonly IHotelService _hotelService;
        private readonly ILogger<C_NFC> _logger;

        public C_NFC(
            IUserBalanceService balanceService,
            ISessionsService sessionsService,
            IVehiclesService vehiclesService,
            IParkinglotsService parkinglotsService,
            IHotelService hotelService,
            ILogger<C_NFC> logger)
        {
            _balanceService = balanceService;
            _sessionsService = sessionsService;
            _vehiclesService = vehiclesService;
            _parkinglotsService = parkinglotsService;
            _hotelService = hotelService;
            _logger = logger;
        }

        // Simulate NFC card read - verify balance and open gate
        [HttpPost("verify-and-pay")]
        public async Task<IActionResult> VerifyAndPay([FromBody] NFCVerifyRequest request)
        {
            try
            {
                // Find user by NFC card ID (in real implementation, this would be a lookup)
                // For now, we'll use user_id directly
                var userId = request.UserId;
                var amount = request.Amount;
                var licensePlate = request.LicensePlate;
                var parkingLotId = request.ParkingLotId;

                // Check if user has sufficient balance
                var hasBalance = await _balanceService.HasSufficientBalance(userId, amount);
                if (!hasBalance)
                {
                    _logger.LogWarning("Insufficient balance for user {UserId}. Required: {Amount}", userId, amount);
                    return BadRequest(new { success = false, message = "Insufficient balance. Please add funds." });
                }

                // Check parking lot capacity
                var parkingLot = await _parkinglotsService.GetById(parkingLotId);
                if (parkingLot == null)
                    return NotFound("Parking lot not found.");

                // Get active sessions and reservations for this parking lot
                var activeSessions = await _sessionsService.GetAll();
                var activeForLot = activeSessions.Where(s => s.parking_lot_id == parkingLotId && (s.stopped == default || s.stopped > DateTime.UtcNow)).ToList();
                var activeReservations = await _parkinglotsService.GetReservationsForLot(parkingLotId);
                var activeResCount = activeReservations.Count(r => r.start_time <= DateTime.UtcNow && r.end_time >= DateTime.UtcNow);

                if (activeForLot.Count + activeResCount >= parkingLot.capacity)
                {
                    _logger.LogWarning("Parking lot {ParkingLotId} is full", parkingLotId);
                    return BadRequest(new { success = false, message = "Parking lot is full." });
                }

                // Check for existing active session for this license plate
                var vehicles = await _vehiclesService.GetAllVehicles();
                var vehicle = vehicles.FirstOrDefault(v => v.license_plate == licensePlate && v.user_id == userId);
                if (vehicle == null)
                    return NotFound("Vehicle not found for this user.");

                var existingSessions = await _sessionsService.GetAll();
                var activeSession = existingSessions.FirstOrDefault(s => s.vehicle_id == vehicle.id && (s.stopped == default || s.stopped > DateTime.UtcNow));
                if (activeSession != null)
                {
                    return BadRequest(new { success = false, message = "Vehicle already has an active session." });
                }

                // Apply hotel discount if applicable
                var discountPercentage = await _hotelService.GetDiscountPercentage(userId);
                var finalAmount = amount;
                if (discountPercentage > 0)
                {
                    finalAmount = amount * (1 - discountPercentage / 100);
                    _logger.LogInformation("Applied {Discount}% hotel discount for user {UserId}", discountPercentage, userId);
                }

                // Deduct balance
                await _balanceService.DeductFromBalance(userId, finalAmount, $"Parking payment for {licensePlate}");

                // Start parking session automatically
                var session = await _sessionsService.Start(new M_Session
                {
                    id = Guid.NewGuid(),
                    user = userId.ToString(),
                    vehicle_id = vehicle.id,
                    parking_lot_id = parkingLotId,
                    started = DateTime.UtcNow,
                    cost = (float)finalAmount
                });

                _logger.LogInformation("NFC payment successful. User {UserId}, Amount: {Amount}, Session: {SessionId}", userId, finalAmount, session.id);

                return Ok(new
                {
                    success = true,
                    message = "Payment successful. Gate opened.",
                    sessionId = session.id,
                    amountCharged = finalAmount,
                    discountApplied = discountPercentage > 0 ? discountPercentage : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing NFC payment");
                return StatusCode(500, new { success = false, message = "Error processing payment." });
            }
        }

        // Verify NFC card and return balance info
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCard([FromBody] NFCVerifyCardRequest request)
        {
            try
            {
                var userId = request.UserId;
                var balance = await _balanceService.GetBalanceAmount(userId);
                var hasBalance = await _balanceService.HasSufficientBalance(userId, request.RequiredAmount);

                return Ok(new
                {
                    success = true,
                    balance = balance,
                    hasSufficientBalance = hasBalance,
                    requiredAmount = request.RequiredAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying NFC card");
                return StatusCode(500, new { success = false, message = "Error verifying card." });
            }
        }
    }

    public class NFCVerifyRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public Guid ParkingLotId { get; set; }
    }

    public class NFCVerifyCardRequest
    {
        public Guid UserId { get; set; }
        public decimal RequiredAmount { get; set; }
    }
}

