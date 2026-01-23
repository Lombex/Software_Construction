using System;
using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IVehiclesService
    {
        Task<List<M_Vehicles>> GetAllVehicles();
        Task<List<M_Vehicles>> GetVehiclesByUserId(Guid userId);
        Task<M_Vehicles> GetByID(Guid id);
        Task CreateVehicle(M_Vehicles newVehicle);
        Task<M_Vehicles> UpdateVehicle(Guid id, M_Vehicles updatedVehicle);
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
        
        public async Task<List<M_Vehicles>> GetVehiclesByUserId(Guid userId)
        {
            return await DbContext.Vehicles
                .Where(v => v.user_id == userId)
                .ToListAsync();
        }
        
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

            // Ensure navigation property is null to prevent EF from trying to insert/update the User entity
            // We only want to set the foreign key (user_id), not the navigation property
            model.M_Users = null;
            
            // Ensure id is set if not already
            if (model.id == Guid.Empty)
                model.id = Guid.NewGuid();
            
            // Set created_at if not set
            if (model.created_at == default)
                model.created_at = DateTime.UtcNow;

            await DbContext.Vehicles.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task<M_Vehicles> UpdateVehicle(Guid id, M_Vehicles updatedVehicle)
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
            _vehicle.year = updatedVehicle.year;
            
            // Ensure navigation property is null to prevent EF from trying to insert/update the User entity
            _vehicle.M_Users = null;
            
            DbContext.Vehicles.Update(_vehicle);
            await DbContext.SaveChangesAsync();
            return _vehicle;
        }
        public async Task DeleteVehicle(Guid id)
        {
            var _vehicle = await GetByID(id);
            DbContext.Vehicles.Remove(_vehicle);
            await DbContext.SaveChangesAsync();
        }
    }
}