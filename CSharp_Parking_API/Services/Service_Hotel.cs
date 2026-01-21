using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    // Interface for hotel service
    public interface IHotelService
    {
        Task<List<M_Hotel>> GetAll();
        Task<M_Hotel?> GetById(Guid id);
        Task<M_Hotel> Create(M_Hotel hotel);
        Task<bool> Update(M_Hotel hotel);
        Task<bool> Delete(Guid id);
        Task<M_HotelGuest> RegisterGuest(Guid hotelId, Guid userId, DateTime checkIn, DateTime? checkOut = null, string? reservationNumber = null);
        Task<bool> CheckOutGuest(Guid guestId);
        Task<List<M_HotelGuest>> GetActiveGuests(Guid hotelId);
        Task<bool> IsHotelGuest(Guid userId);
        Task<decimal> GetDiscountPercentage(Guid userId);
    }

    // Service for managing hotels and hotel guests
    public class S_Hotel : IHotelService
    {
        private readonly SQLite_Database DbContext;

        public S_Hotel(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }

        public async Task<List<M_Hotel>> GetAll() =>
            await DbContext.Hotels.AsNoTracking().Where(h => h.active).ToListAsync();

        public async Task<M_Hotel?> GetById(Guid id) =>
            await DbContext.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.id == id && h.active);

        public async Task<M_Hotel> Create(M_Hotel hotel)
        {
            hotel.id = hotel.id == Guid.Empty ? Guid.NewGuid() : hotel.id;
            hotel.created_at = hotel.created_at == default ? DateTime.UtcNow : hotel.created_at;
            hotel.active = true;

            await DbContext.Hotels.AddAsync(hotel);
            await DbContext.SaveChangesAsync();
            return hotel;
        }

        public async Task<bool> Update(M_Hotel hotel)
        {
            var existing = await DbContext.Hotels.FindAsync(hotel.id);
            if (existing == null) return false;

            existing.name = hotel.name;
            existing.address = hotel.address;
            existing.phone = hotel.phone;
            existing.email = hotel.email;
            existing.discount_percentage = hotel.discount_percentage;

            DbContext.Hotels.Update(existing);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var hotel = await DbContext.Hotels.FindAsync(id);
            if (hotel == null) return false;

            hotel.active = false; // Soft delete
            DbContext.Hotels.Update(hotel);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<M_HotelGuest> RegisterGuest(Guid hotelId, Guid userId, DateTime checkIn, DateTime? checkOut = null, string? reservationNumber = null)
        {
            // Check if user is already a guest
            var existing = await DbContext.HotelGuests
                .FirstOrDefaultAsync(hg => hg.user_id == userId && hg.hotel_id == hotelId && hg.check_out == null);
            if (existing != null)
                throw new InvalidOperationException("User is already registered as a guest at this hotel.");

            var guest = new M_HotelGuest
            {
                id = Guid.NewGuid(),
                hotel_id = hotelId,
                user_id = userId,
                check_in = checkIn,
                check_out = checkOut,
                reservation_number = reservationNumber,
                discount_applied = false,
                created_at = DateTime.UtcNow
            };

            await DbContext.HotelGuests.AddAsync(guest);
            await DbContext.SaveChangesAsync();
            return guest;
        }

        public async Task<bool> CheckOutGuest(Guid guestId)
        {
            var guest = await DbContext.HotelGuests.FindAsync(guestId);
            if (guest == null) return false;

            guest.check_out = DateTime.UtcNow;
            DbContext.HotelGuests.Update(guest);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<M_HotelGuest>> GetActiveGuests(Guid hotelId)
        {
            return await DbContext.HotelGuests
                .AsNoTracking()
                .Where(hg => hg.hotel_id == hotelId && hg.check_out == null)
                .ToListAsync();
        }

        // Check if user is currently a hotel guest
        public async Task<bool> IsHotelGuest(Guid userId)
        {
            return await DbContext.HotelGuests
                .AnyAsync(hg => hg.user_id == userId && hg.check_out == null);
        }

        // Get discount percentage for a user (if they are a hotel guest)
        public async Task<decimal> GetDiscountPercentage(Guid userId)
        {
            var activeGuest = await DbContext.HotelGuests
                .FirstOrDefaultAsync(hg => hg.user_id == userId && hg.check_out == null);

            if (activeGuest == null) return 0;

            var hotel = await GetById(activeGuest.hotel_id);
            return hotel?.discount_percentage ?? 0;
        }
    }
}

