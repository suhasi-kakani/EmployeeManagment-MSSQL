using EmployeeManagment.Dtos;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Dtos;
using System.Security.Claims;
using static Azure.Core.HttpHeader;

namespace EmployeeManagment.Interfaces
{
    public interface IEmployeeService
    {
        Task<Employee> CreateEmployee(EmployeeRequest request);
        Task<IEnumerable<Employee>> GetEmployees();

        Task<Employee> GetEmployeeById(string id);
        Task<Employee> UpdateEmployeeBasic(string id, EmployeeRequest request);
        Task<bool> DeleteEmployee(string id);

        Task<Employee> UpdateAddress(string id, AddressDto address);
        Task<Employee> UpdateEmploymentHistory(string id, List<EmploymentDto> histories);

        Task<Employee> GetEmployee(ClaimsPrincipal principal);

        Task<List<EmployeeSummaryDto>> GetAllEmployeesBasic();

        Task<(List<EmployeeSummaryDto>, int totalCount)> GetEmployeesPaged(
            int totalCount,
            int pageSize = 5,
            string sortBy = "name",
            bool ascending = true);

    }
}
