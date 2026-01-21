using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    public interface IUsersService
    {
        Task<List<M_Users>> GetAllUsers();
        Task<M_Users> getByID(Guid id);
        Task CreateUser(M_Users newUser);
        Task UpdateProfile(Guid id, M_Users updatedUser);
        Task DeleteUser(Guid id);
    }

    public class S_Users : IUsersService
    {
        private readonly SQLite_Database DbContext;
        public S_Users(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }
        public async Task<List<M_Users>> GetAllUsers() => await DbContext.Users.AsQueryable().ToListAsync();
        public async Task<M_Users> getByID(Guid id)
        {
            var user = await DbContext.Users.FirstOrDefaultAsync(x => x.id == id);
            return user!;
        }
        public async Task CreateUser(M_Users model)
        {
            await DbContext.Users.AddAsync(model);
            await DbContext.SaveChangesAsync();
        }
        public async Task UpdateProfile(Guid id, M_Users updatedUser)
        {
            var _user = await getByID(id);
            _user.username = updatedUser.username;
            _user.password = updatedUser.password;
            _user.name = updatedUser.name;
            _user.email = updatedUser.email;
            _user.phone = updatedUser.phone;
            _user.role = updatedUser.role;
            _user.birth_year = updatedUser.birth_year;
            _user.active = updatedUser.active;
            DbContext.Users.Update(_user);
            await DbContext.SaveChangesAsync();
        }
        public async Task DeleteUser(Guid id)
        {
            var _user = await getByID(id);
            DbContext.Users.Remove(_user);
            await DbContext.SaveChangesAsync();
        }
    }
}
