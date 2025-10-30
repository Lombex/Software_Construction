using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IVehiclesService
    {
        Task<List<Vehicle>> GetAllVehicles();
        Task<Vehicle> GetByID(Guid id);
        Task CreateVehicle(Vehicle newVehicle);
        Task UpdateVehicle(Guid id, Vehicle updatedVehicle);
        Task DeleteVehicle(Guid id);
    }

    public class S_Vehicles : IVehiclesService
    {
        private readonly SQLite_Database DbContext;
        public S_Vehicles(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }
        
        public async Task<List<Vehicle>> GetAllVehicles() => await DbContext.Vehicles.AsQueryable().ToListAsync();
        public async Task<Vehicle> GetByID(Guid id)
        {
            var vehicle = await DbContext.Vehicles.FirstOrDefaultAsync(x => x.id == id);
            if (vehicle == null) throw new Exception("Vehicle has not been found!");
            return vehicle;
        }
        public async Task CreateVehicle(Vehicle model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            await DbContext.Vehicles.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task UpdateVehicle(Guid id, Vehicle updatedVehicle)
        {
            var _vehicle = await GetByID(id);
            _vehicle.license_plate = updatedVehicle.license_plate;
            _vehicle.model = updatedVehicle.model;
            _vehicle.color = updatedVehicle.color;
            _vehicle.owner_id = updatedVehicle.owner_id;
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