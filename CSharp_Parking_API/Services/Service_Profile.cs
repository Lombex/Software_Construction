using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using static CSharpAPI.Models.M_Reservations;

namespace CSharpAPI.Services
{
    public interface IProfileService
    {
        Task<M_Users?> GetById(Guid id);
        Task<M_Users> UpdateProfile(Guid id);
        Task<M_Users> ChangePassword(Guid id, string newPassword);
        Task<bool> DeleteProfile(Guid id);
    }

    public class Service_Profile : IProfileService
    {
        private readonly SQLite_Database DbContext;
        public Service_Profile(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }
        public async Task<M_Users?> GetById(Guid id)
        {
            var user = await DbContext.Users.FirstOrDefaultAsync(x => x.id == id);
            if (user == null) throw new Exception("User has not been found!");
            return user;
        }
        public async Task<M_Users> UpdateProfile(Guid id)
        {
            var _user = await GetById(id);
            DbContext.Users.Update(_user);
            await DbContext.SaveChangesAsync();
            return _user;
        }
        public async Task<M_Users> ChangePassword(Guid id, string newPassword)
        {
            var user = await GetById(id);
            if (user == null) throw new Exception("User not found");

            user.password = newPassword; // In real applications, hash the password
            DbContext.Users.Update(user);
            await DbContext.SaveChangesAsync();
            return user;
        }
        public async Task<bool> DeleteProfile(Guid id)
        {
            var _user = await GetById(id);
            DbContext.Users.Remove(_user);
            await DbContext.SaveChangesAsync();
            return true;
        }
        
    }
    


}