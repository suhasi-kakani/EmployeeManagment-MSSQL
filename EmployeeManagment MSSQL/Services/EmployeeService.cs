using Azure.Core;
using EmployeeManagment.Dtos;
using EmployeeManagment.Interfaces;
using EmployeeManagment.Models;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagment.Repository;
using EmployeeManagment_MSSQL.Dtos;
using EmployeeManagment_MSSQL.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualBasic;
using User = EmployeeManagment.Models.User;

namespace EmployeeManagment.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository employeeRepository;
        private readonly IUserRepository userRepository;

        public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository)
        {
            this.employeeRepository = employeeRepository;
            this.userRepository = userRepository;
        }
        public async Task<Employee> CreateEmployee(EmployeeRequest request)
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Email = request.Email,
                Designation = request.Designation,
                Department = request.Department,
                ContactNumber = request.ContactNumber,
                Address = new Address(), 
                Employments = new List<EmploymentHistory>()
            };

            var newEmployee = await employeeRepository.CreateEmployee(employee);

            var user = new User
            {
                EmployeeId = employee.Id,
                Role = "Employee",
                Id = Guid.NewGuid().ToString(),
                Username = employee.Name,
                PasswordHash = HashPassword("Default@123"),
            };

            await userRepository.CreateUser(user);
            return newEmployee;
        }

        private string HashPassword(string v)
        {
           return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(v)));
        }

        public async Task<IEnumerable<Employee>> GetEmployees()
        {
            var response = await employeeRepository.GetAll();
            return response;

        }

        public async Task<Employee> GetEmployeeById(string id)
        {
            var response = await employeeRepository.GetById(id);
            return response;
        }

        public async Task<Employee?> GetEmployee(ClaimsPrincipal id)
        {
            var userId = id.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = id.FindFirstValue(ClaimTypes.Role);
            
            if (userId == null)
                return null;
            var userResponse = await userRepository.GetById(userId);

            var user = userResponse;

            if (user?.EmployeeId == null) return null;

            var response =  await employeeRepository.GetById(user.EmployeeId);
            return response;
        }

        public async Task<Employee> UpdateEmployeeBasic(string id, EmployeeRequest request)
        {
            var employee = await GetEmployeeById(id);
            if (employee == null) return null;

            employee.Name = request.Name;
            employee.Email = request.Email;
            employee.Designation = request.Designation;
            employee.Department = request.Department;
            employee.ContactNumber = request.ContactNumber;
            employee.UpdatedAt = DateTime.UtcNow;

            var response = await employeeRepository.Update(employee);
            return response;
        }

        public async Task<bool> DeleteEmployee(string id)
        {
            var response = await employeeRepository.SoftDelete(id);
            return response;
        }

        public async Task<Employee> UpdateAddress(string id, AddressDto address)
        {
            var employee = await GetEmployeeById(id);
            if (employee == null) return null;

            employee.Address.Street = address.Street;
            employee.Address.City = address.City;
            employee.Address.State = address.State;
            employee.Address.Country = address.Country;
            employee.Address.PostalCode = address.PostalCode;
            employee.Address.EmployeeId = id;
            employee.UpdatedAt = DateTime.UtcNow;

            var response = await employeeRepository.Update(employee);
            return response;
        }

        public async Task<Employee> UpdateEmploymentHistory(string id, List<EmploymentDto> histories)
        {
            var employee = await GetEmployeeById(id);
            if (employee == null) return null;

            if (employee.Employments == null)
                employee.Employments = new List<EmploymentHistory>();

            foreach (var dto in histories)
            {
                var existing = employee.Employments.FirstOrDefault(e =>
                    e.CompanyName == dto.CompanyName &&
                    e.JobTitle == dto.JobTitle &&
                    e.startDate == dto.startDate);

                if (existing != null)
                {
                    existing.CompanyName = dto.CompanyName;
                    existing.JobTitle = dto.JobTitle;
                    existing.startDate = dto.startDate;
                    existing.endDate = dto.endDate;
                }
                else
                {
                    employee.Employments.Add(new EmploymentHistory
                    {
                        CompanyName = dto.CompanyName,
                        JobTitle = dto.JobTitle,
                        startDate = dto.startDate,
                        endDate = dto.endDate,
                        EmployeeId = id
                    });
                }
            }

            employee.UpdatedAt = DateTime.UtcNow;
            var response = await employeeRepository.Update(employee);
            return response;
        }

        public async Task<List<EmployeeSummaryDto>> GetAllEmployeesBasic()
        {
            var response = await employeeRepository.GetAllBasics();
            return response;
        }

        public async Task<(List<EmployeeSummaryDto>, int totalCount)> GetEmployeesPaged(
            int pageNumber = 1,
            int pageSize = 3,
            string sortBy = "name",
            bool ascending = true)
        {
            var (result, totalCount) = await employeeRepository.GetPaged(pageNumber, pageSize, sortBy,
                ascending);

            return (result, totalCount);
        }
    }
}
