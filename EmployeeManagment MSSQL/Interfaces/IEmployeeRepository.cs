using EmployeeManagment.Dtos;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Exceptions;

namespace EmployeeManagment_MSSQL.Interfaces
{
    public interface IEmployeeRepository
    {
        public Task<Result<Employee>> CreateEmployee(Employee employee);
        public Task<Result<Employee>> GetById(string id);
        public Task<Result<IEnumerable<Employee>>> GetAll();
        public Task<Result<Employee>> Update(Employee employee);

        public Task<Result> SoftDelete(string id);
        public Task<Result<List<EmployeeSummaryDto>>> GetAllBasics();

        public Task<Result<(List<EmployeeSummaryDto>, int totalCount)>> GetPaged(int pageNumber, int pageSize,
            string sortBy, bool ascending);
    }
}
