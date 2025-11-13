using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IPaymentsService
    {
        Task<List<M_Payments>> GetAllPayments();
        Task<M_Payments> getByID(Guid id);
        Task CreatePayment(M_Payments newPayment);
        Task UpdatePayment(Guid id, M_Payments updatedPayment);
        Task DeletePayment(Guid id);
    }

    public class S_Payments : IPaymentsService
    {
        private readonly SQLite_Database DbContext;
        public S_Payments(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }
        
        public async Task<List<M_Payments>> GetAllPayments() => await DbContext.Payments.AsQueryable().ToListAsync();
        public async Task<M_Payments> getByID(Guid id)
        {
            var payment = await DbContext.Payments.FirstOrDefaultAsync(x => x.id == id);
            if (payment == null) throw new Exception("Payment has not been found!");
            return payment;
        }
        public async Task CreatePayment(M_Payments model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await DbContext.Payments.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task UpdatePayment(Guid id, M_Payments updatedPayment)
        {
            var _payment = await getByID(id);
            _payment.id = updatedPayment.id;
            _payment.reservation_id = updatedPayment.reservation_id;
            _payment.paid_at = updatedPayment.paid_at;
            _payment.transactions = updatedPayment.transactions;
            _payment.initiator = updatedPayment.initiator;
            _payment.amount = updatedPayment.amount;
            _payment.created_at = updatedPayment.created_at;
            _payment.completed = updatedPayment.completed;
            _payment.hash = updatedPayment.hash;
            _payment.t_data = updatedPayment.t_data;
            _payment.session_id = updatedPayment.session_id;
            _payment.parking_lot_id = updatedPayment.parking_lot_id;
            DbContext.Payments.Update(_payment);
            await DbContext.SaveChangesAsync();
        }
        public async Task DeletePayment(Guid id)
        {
            var _payment = await getByID(id);
            DbContext.Payments.Remove(_payment);
            await DbContext.SaveChangesAsync();
        }
    }
}