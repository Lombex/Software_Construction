using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IVehiclesService
    {
        Task<List<M_Vehicles>> GetAllVehicles();
        Task<M_Vehicles> GetByID(Guid id);
        Task CreateVehicle(M_Vehicles newVehicle);
        Task UpdateVehicle(Guid id, M_Vehicles updatedVehicle);
        Task DeleteVehicle(Guid id);
    }

    public class S_Vehicles : IVehiclesService
    {
        private readonly SQLite_Database DbContext;
        public S_Vehicles(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }
        
        public async Task<List<M_Vehicles>> GetAllVehicles() => await DbContext.Vehicles.AsQueryable().ToListAsync();
        public async Task<M_Vehicles> GetByID(Guid id)
        {
            var vehicle = await DbContext.Vehicles.FirstOrDefaultAsync(x => x.id == id);
            return vehicle!;
        }
        public async Task CreateVehicle(M_Vehicles model)
        {
            // Validate unique license plate per user
            var existingVehicle = await DbContext.Vehicles
                .FirstOrDefaultAsync(v => v.user_id == model.user_id && 
                                         v.license_plate != null && 
                                         v.license_plate.ToUpper() == model.license_plate!.ToUpper());
            
            if (existingVehicle != null)
                throw new InvalidOperationException($"License plate '{model.license_plate}' already exists for this user.");

            await DbContext.Vehicles.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task UpdateVehicle(Guid id, M_Vehicles updatedVehicle)
        {
            var _vehicle = await GetByID(id);

            // If license plate is being changed, validate uniqueness
            if (updatedVehicle.license_plate != null && 
                updatedVehicle.license_plate.ToUpper() != _vehicle.license_plate?.ToUpper())
            {
                var existingVehicle = await DbContext.Vehicles
                    .FirstOrDefaultAsync(v => v.user_id == _vehicle.user_id && 
                                             v.id != id &&
                                             v.license_plate != null && 
                                             v.license_plate.ToUpper() == updatedVehicle.license_plate.ToUpper());
                
                if (existingVehicle != null)
                    throw new InvalidOperationException($"License plate '{updatedVehicle.license_plate}' already exists for this user.");
            }

            _vehicle.license_plate = updatedVehicle.license_plate;
            _vehicle.make = updatedVehicle.make;
            _vehicle.model = updatedVehicle.model;
            _vehicle.color = updatedVehicle.color;
            _vehicle.user_id = updatedVehicle.user_id;
            DbContext.Vehicles.Update(_vehicle);
            await DbContext.SaveChangesAsync();
        }
        public async Task DeleteVehicle(Guid id)
        {
            var _vehicle = await GetByID(id);
            DbContext.Vehicles.Remove(_vehicle);
            await DbContext.SaveChangesAsync();
        }
    }
}