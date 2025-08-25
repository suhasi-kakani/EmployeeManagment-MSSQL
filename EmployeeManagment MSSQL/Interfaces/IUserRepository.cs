using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Exceptions;

namespace EmployeeManagment_MSSQL.Interfaces
{
    public interface IUserRepository
    {
        public Task<Result<User>> CreateUser(User user);
        public Task<Result<User>> LoginUser(string username, string password);
        public Task<Result<User>> GetById(string id);
        public Task<Result<List<User>>> GetActiveUsers();
        public Task<Result> UpdatePassword(string id, string newPassword);
    }
}
