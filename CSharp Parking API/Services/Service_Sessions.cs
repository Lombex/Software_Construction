using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Services
{
    public interface ISessionsService
    {
        Task<M_Session> Start(M_Session session);
        Task<M_Session?> Stop(Guid id);
        Task<List<M_Session>> GetSessionById(string user);
        Task<List<M_Session>> GetAllSessions(string user, M_Session.PaymentStatus status);
        Task<List<M_Session>> GetAll();
        Task<M_Session>? Pay(Guid id);
    }

    public class S_Sessions : ISessionsService
    {
        private readonly SQLite_Database DbContext;
        public S_Sessions(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<M_Session> Start(M_Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            // Check if there's already an active session for this license plate
            var existingSessions = await DbContext.Sessions
                .Where(s => 
                    (s.license_plate != null && s.license_plate == session.license_plate) ||
                    (s.vehicle_id == session.vehicle_id && s.vehicle_id != Guid.Empty))
                .ToListAsync();

            // Check if any session is active (stopped time is in the future or not set)
            var activeSession = existingSessions
                .FirstOrDefault(s => s.stopped == default || s.stopped > DateTime.UtcNow);

            if (activeSession != null)
            {
                Log.Warning("Attempted to start session for license plate {LicensePlate} but active session already exists", session.license_plate);
                throw new InvalidOperationException($"There is already an active session for license plate '{session.license_plate}'. Please stop the existing session first.");
            }

            // Check parking lot capacity
            var parkingLot = await DbContext.Parkinglots.FindAsync(session.parking_lot_id);
            if (parkingLot == null)
            {
                Log.Error("Parking lot {ParkingLotId} not found when starting session", session.parking_lot_id);
                throw new InvalidOperationException("Parking lot not found.");
            }

            // Count active sessions for this parking lot
            var activeSessionsCount = await DbContext.Sessions
                .CountAsync(s => s.parking_lot_id == session.parking_lot_id && 
                               (s.stopped == default || s.stopped > DateTime.UtcNow));

            // Count active reservations for this parking lot
            var activeReservationsCount = await DbContext.Reservations
                .CountAsync(r => r.parking_lot_id == session.parking_lot_id && 
                               r.status == M_Reservations.Status.Active &&
                               r.start_time <= DateTime.UtcNow && 
                               r.end_time > DateTime.UtcNow);

            var totalOccupied = activeSessionsCount + activeReservationsCount;
            if (totalOccupied >= parkingLot.capacity)
            {
                Log.Warning("Parking lot {ParkingLotName} is full. Capacity: {Capacity}, Occupied: {Occupied}", 
                    parkingLot.name, parkingLot.capacity, totalOccupied);
                throw new InvalidOperationException($"Parking lot '{parkingLot.name}' is full. Capacity: {parkingLot.capacity}, Occupied: {totalOccupied}");
            }

            // Set started time if not set
            if (session.started == default)
                session.started = DateTime.UtcNow;

            await DbContext.Sessions.AddAsync(session);
            await DbContext.SaveChangesAsync();
            Log.Information("Session {SessionId} started for license plate {LicensePlate} at parking lot {ParkingLotId}", 
                session.id, session.license_plate, session.parking_lot_id);
            return session;
        }
        public async Task<M_Session?> Stop(Guid id)
        {
            var session = await DbContext.Sessions.FindAsync(id);
            if (session == null) throw new Exception("Session not found");
            session.stopped = DateTime.UtcNow;
            session.duration_minutes = (int)(session.stopped - session.started).TotalMinutes;
            // Example cost calculation
            session.cost = session.duration_minutes * 0.05f; // e.g., 5 cents per minute
            session.status = M_Session.PaymentStatus.Unpaid;
            DbContext.Sessions.Update(session);
            await DbContext.SaveChangesAsync();
            return session;
        }

        public async Task<List<M_Session>> GetSessionById(string user)
        {
            var query = DbContext.Sessions.AsQueryable().Where(s => s.user == user);
            return await query.ToListAsync();
        }

        public async Task<List<M_Session>> GetAllSessions(string user, M_Session.PaymentStatus status)
        {
            var query = DbContext.Sessions.AsQueryable().Where(s => s.user == user && s.status == status);
            return await query.ToListAsync();
        }

        public async Task<List<M_Session>> GetAll()
        {
            return await DbContext.Sessions.AsNoTracking().ToListAsync();
        }

        public async Task<M_Session>? Pay(Guid id)
        {
            var session = await DbContext.Sessions.FindAsync(id);
            if (session == null) throw new Exception("Session not found");
            session.status = M_Session.PaymentStatus.Paid;
            DbContext.Sessions.Update(session);
            await DbContext.SaveChangesAsync();
            return session;
        }
    }
}