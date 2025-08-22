using EmployeeManagment.Dtos;
using EmployeeManagment.Models;

namespace EmployeeManagment_MSSQL.Interfaces
{
    public interface IEmployeeRepository
    {
        public Task<Employee> CreateEmployee(Employee employee);
        public Task<Employee> GetById(string id);
        public Task<IEnumerable<Employee>> GetAll();
        public Task<Employee> Update(Employee employee);

        public Task<bool> SoftDelete(string id);
        public Task<List<EmployeeSummaryDto>> GetAllBasics();

        public Task<(List<EmployeeSummaryDto>, int totalCount)> GetPaged(int pageNumber, int pageSize,
            string sortBy, bool ascending);
    }
}
