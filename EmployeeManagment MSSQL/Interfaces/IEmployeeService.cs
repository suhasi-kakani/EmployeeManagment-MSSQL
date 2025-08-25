using EmployeeManagment.Dtos;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Dtos;
using System.Security.Claims;
using EmployeeManagment_MSSQL.Exceptions;
using static Azure.Core.HttpHeader;

namespace EmployeeManagment.Interfaces
{
    public interface IEmployeeService
    {
        Task<Result<Employee>> CreateEmployee(EmployeeRequest request);
        Task<Result<IEnumerable<Employee>>> GetEmployees();

        Task<Result<Employee>> GetEmployeeById(string id);
        Task<Result<Employee>> UpdateEmployeeBasic(string id, EmployeeRequest request);
        Task<Result> DeleteEmployee(string id);

        Task<Result<Employee>> UpdateAddress(string id, AddressDto address);
        Task<Result<Employee>> UpdateEmploymentHistory(string id, List<EmploymentDto> histories);

        Task<Result<Employee>> GetEmployee(ClaimsPrincipal principal);

        Task<Result<List<EmployeeSummaryDto>>> GetAllEmployeesBasic();

        Task<Result<(List<EmployeeSummaryDto>, int totalCount)>> GetEmployeesPaged(
            int totalCount,
            int pageSize = 5,
            string sortBy = "name",
            bool ascending = true);

    }
}
