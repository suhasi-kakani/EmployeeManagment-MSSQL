using System.Security.Cryptography;
using System.Text;
using EmployeeManagment_MSSQL.Data;
using EmployeeManagment_MSSQL.Interfaces;
using Microsoft.EntityFrameworkCore;
using User = EmployeeManagment.Models.User;

namespace EmployeeManagment.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext context;

        public UserRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<User> CreateUser(User user)
        {
           context.Users.Add(user);
           await context.SaveChangesAsync();
           return user;
        }

        public async Task<User?> LoginUser(string username, string password)
        {
            var user = await context.Users.Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

            if (hash != user.PasswordHash)
                return null;

            return user;
        }


        public async Task<User?> GetById(string id)
        {
            return await context.Users.Include(e => e.Employee).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetActiveUsers()
        {
            return await context.Users.Include(e => e.Employee)
                .Where(u => u.Employee != null && u.Employee.IsWorking == true)
                .ToListAsync();
        }

        public async Task<bool> UpdatePassword(string id, string newPassword)
        {
            var user = await context.Users.FindAsync(id);
            if(user == null) return false;
            user.PasswordHash = newPassword;
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
