using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Services
{
    public record Availability(bool IsAvailable, string? Reason = null);
    public interface IReservationsService
    {
        Task<M_Reservations> Create(M_Reservations res);
        Task Cancel(Guid id);
        Task<M_Reservations?> GetById(Guid id);
        Task<List<M_Reservations>> ListByUser(Guid userId, Status status);
        Task<Availability> CheckAvailability(Guid parkingLotId, DateTime from, DateTime to);
        Task<List<M_Reservations>> GetAllReservations();
    }

    public class S_Reservations : IReservationsService
    {
        private readonly SQLite_Database DbContext;
        public S_Reservations(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }
        public async Task<M_Reservations> Create(M_Reservations res)
        {
            if (res == null) throw new ArgumentNullException(nameof(res));
            // Ensure only FK fields are used when creating; navigations should not be inserted.
            await DbContext.Reservations.AddAsync(res);
            await DbContext.SaveChangesAsync();
            return res;
        }

        public async Task Cancel(Guid id)
        {
            var reservation = await GetById(id);
            if (reservation == null) throw new Exception("Reservation not found");
            reservation.status = M_Reservations.Status.Cancelled;
            DbContext.Reservations.Update(reservation);
            await DbContext.SaveChangesAsync();
        }
        public async Task<M_Reservations?> GetById(Guid id) => await DbContext.Reservations.FindAsync(id);

        public async Task<List<M_Reservations>> ListByUser(Guid userId, Status status)
        {
            var query = DbContext.Reservations.AsQueryable().Where(r => r.user_id == userId);
            return await query.ToListAsync();
        }

        public async Task<Availability> CheckAvailability(Guid parkingLotId, DateTime from, DateTime to)
        {
            var overlappingReservations = await DbContext.Reservations
                .Where(r => r.parking_lot_id == parkingLotId &&
                            r.status == M_Reservations.Status.Active &&
                            r.start_time < to &&
                            r.end_time > from)
                .ToListAsync();

            if (overlappingReservations.Any())
            {
                return new Availability(false, "The parking lot is not available for the selected time range.");
            }

            return new Availability(true);
        }

        public async Task<List<M_Reservations>> GetAllReservations() => await DbContext.Reservations.AsQueryable().ToListAsync();

    }


}