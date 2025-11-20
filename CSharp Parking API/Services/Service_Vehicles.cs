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
            await DbContext.Vehicles.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task UpdateVehicle(Guid id, M_Vehicles updatedVehicle)
        {
            var _vehicle = await GetByID(id);
            _vehicle.license_plate = updatedVehicle.license_plate;
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