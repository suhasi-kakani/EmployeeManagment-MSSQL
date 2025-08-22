using EmployeeManagment.Models;

namespace EmployeeManagment_MSSQL.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> CreateUser(User user);
        public Task<User?> LoginUser(string username, string password);
        public Task<User?> GetById(string id);
        public Task<List<User>> GetActiveUsers();
        public Task<bool> UpdatePassword(string id, string newPassword);
    }
}
