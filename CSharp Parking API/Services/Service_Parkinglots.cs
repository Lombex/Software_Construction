using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IParkinglotsService
    {
        Task<List<M_Parkinglots>> GetAllParkinglots(); //
        Task<M_Parkinglots> GetById(Guid id); //
        Task CreateParkinglot(M_Parkinglots newParkinglot); //
        Task UpdateParkinglot(Guid id, M_Parkinglots updatedParkinglot); //
        Task DeleteParkinglot(Guid id); //

        // Task<List<M_Parkinglots>> SearchNearbyParkinglots((double lat, double lng) centerCoordinates, int radiusM, string? query);
        // Task<List<M_Tariff>>GetRatesParkinglot(Guid parkingLotId);
        // Task<List<M_Tariff>>UpdateRatesParkinglot(Guid parkingLotId, List<M_Tariff> rates);
    }

    public class S_Parkinglots : IParkinglotsService
    {
        private readonly SQLite_Database DbContext;
        public S_Parkinglots(SQLite_Database dbContext)

        {
            DbContext = dbContext;
        }

        public async Task<List<M_Parkinglots>> GetAllParkinglots() => await DbContext.Parkinglots.AsQueryable().ToListAsync();
        
        public async Task<M_Parkinglots> GetById(Guid id)
        {
            var lot = await DbContext.Parkinglots.FirstOrDefaultAsync(x => x.id == id);
            if (lot == null) throw new Exception("Parking lot has not been found!");
            
            return lot;
        }

        public async Task CreateParkinglot(M_Parkinglots model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await DbContext.Parkinglots.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task UpdateParkinglot(Guid id, M_Parkinglots updatedParkinglot)
        {
            var _parkinglot = await GetById(id);
            _parkinglot.name = updatedParkinglot.name;
            _parkinglot.location = updatedParkinglot.location;
            _parkinglot.address = updatedParkinglot.address;
            _parkinglot.capacity = updatedParkinglot.capacity;
            _parkinglot.reserved = updatedParkinglot.reserved;
            _parkinglot.daytarriff = updatedParkinglot.daytarriff;
            _parkinglot.created_at = updatedParkinglot.created_at;
            _parkinglot.coordinates = updatedParkinglot.coordinates;

            DbContext.Parkinglots.Update(_parkinglot);
            await DbContext.SaveChangesAsync();
        }

        public async Task DeleteParkinglot(Guid id)
        {
            var _parkinglot = await GetById(id);
            DbContext.Parkinglots.Remove(_parkinglot);
            await DbContext.SaveChangesAsync();
        }

        public async Task<List<M_Parkinglots>> SearchNearbyParkinglots((double lat, double lng) centerCoordinates, int radiusM, string? query)
        {
            // placeholder implementation:  returning all lots for now
            var lots = await DbContext.Parkinglots.ToListAsync();
            
            if (!string.IsNullOrEmpty(query))
            {
                lots = lots.Where(l => l.name !=null && l.name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return lots;
        }

     

        /*public async Task<List<M_Tariff>> GetRatesParkinglot(Guid parkinglotId)
        {
           var rates = await DbContext.ParkinglotRates
                .FirstOrDefaultAsync(r => r.parkinglotId == parkinglotId);

            if (rates == null)
               return new List<M_Tariff>();
            
            return rates.tariffs;
        }

        public async Task<List<M_Tariff>> UpdateRatesParkinglot(Guid parkinglotId, List<M_Tariff> rates)
        {
           var existingRates = await DbContext.ParkinglotRates
                .FirstOrDefaultAsync(r => r.parkinglotId == parkinglotId);

            if (existingRates == null)
            {
                existingRates = new M_ParkinglotRates
                {
                    parkinglotId = parkinglotId,
                    tariffs = rates
                };
                await DbContext.ParkinglotRates.AddAsync(existingRates);
            }
            else
            {
                existingRates.tariffs = rates;
                DbContext.ParkinglotRates.Update(existingRates);
            }

            await DbContext.SaveChangesAsync();
            return existingRates.tariffs;
        }*/
        /*
        {
            return await _context.Parkinglots.ToListAsync();
        }

        public async Task<M_Parkinglots?> GetById(Guid id)
        {
            return await _context.Parkinglots.FindAsync(id);
        }

       */
    }   

}