using System.Security.Cryptography;
using System.Text;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Data;
using EmployeeManagment_MSSQL.Exceptions;
using EmployeeManagment_MSSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagment.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext context;

        public UserRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Result<User>> CreateUser(User user)
        {
            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
                return Result<User>.Success(user);
            }
            catch (Exception e)
            {
                return Result<User>.Failure($"Failed to create user : {e.Message}");
            }
        }

        public async Task<Result<User?>> LoginUser(string username, string password)
        {
            try
            {
                var user = await context.Users.Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return Result<User>.Failure("User not found");
                }

                var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

                if (hash != user.PasswordHash)
                    return Result<User>.Failure("Invalid Password");

                return Result<User>.Success(user);
            }
            catch (Exception e)
            {
                return Result<User?>.Failure($"Failed to login user: {e.Message}");
            }
        }


        public async Task<Result<User?>> GetById(string id)
        {
            try
            {
                var user =  await context.Users.Include(e => e.Employee).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return Result<User>.Failure("User not found");
                }

                return Result<User>.Success(user);
            }
            catch (Exception e)
            {
                return Result<User>.Failure($"Failed to retrive user: {e.Message}");
            }
        }

        public async Task<Result<List<User>>> GetActiveUsers()
        {
            try
            {
                var user =  await context.Users.Include(e => e.Employee)
                    .Where(u => u.Employee != null && u.Employee.IsWorking == true)
                    .ToListAsync();
                return Result<List<User>>.Success(user);
            }
            catch (Exception e)
            {
                return Result<List<User>>.Failure($"Failed to retrive active users: {e.Message}");
            }
        }

        public async Task<Result> UpdatePassword(string id, string newPassword)
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if(user == null) return Result.Failure("User not found");
                user.PasswordHash = newPassword;
                context.Users.Update(user);
                await context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Failure($"Failed to update password: {e.Message} ");
            }
        }
    }
}
