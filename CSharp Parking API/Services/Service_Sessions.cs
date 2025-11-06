using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Services
{
    public interface ISessionsService
    {
        Task<M_Session> Start(M_Session session);
        Task<M_Session?> Stop(Guid id);
        Task<List<M_Session>> GetSessionById(string user);
        Task<List<M_Session>> GetAllSessions(string user, M_Session.PaymentStatus status);
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
            await DbContext.Sessions.AddAsync(session);
            await DbContext.SaveChangesAsync();
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