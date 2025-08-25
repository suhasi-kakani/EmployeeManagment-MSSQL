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
using EmployeeManagment_MSSQL.Exceptions;
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

        public async Task<Result<Employee>> CreateEmployee(EmployeeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email))
                {
                    return Result<Employee>.Failure("Name and email are required.");
                }

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

                var employeeResult = await employeeRepository.CreateEmployee(employee);

                if (!employeeResult.IsSuccess)
                {
                    return Result<Employee>.Failure(employeeResult.ErrorMessage);
                }

                var user = new User
                {
                    EmployeeId = employee.Id,
                    Role = "Employee",
                    Id = Guid.NewGuid().ToString(),
                    Username = employee.Name,
                    PasswordHash = HashPassword("Default@123"),
                };

                var userResult = await userRepository.CreateUser(user);
                if (!userResult.IsSuccess)
                {
                    // Optionally, rollback employee creation if user creation fails
                    await employeeRepository.SoftDelete(employee.Id);
                    return Result<Employee>.Failure($"Failed to create user: {userResult.ErrorMessage}");
                }

                return Result<Employee>.Success(employeeResult.Value);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to create user: {e.Message}");
            }
        }

        private string HashPassword(string v)
        {
           return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(v)));
        }

        public async Task<Result<IEnumerable<Employee>>> GetEmployees()
        {
            try
            {
                var response = await employeeRepository.GetAll();
                if (!response.IsSuccess)
                {
                    return Result<IEnumerable<Employee>>.Failure(response.ErrorMessage);
                }
                return Result<IEnumerable<Employee>>.Success(response.Value);
            }
            catch (Exception e)
            {
               return Result<IEnumerable<Employee>>.Failure($"Failed to retrieve employees: {e.Message}");
            }

        }

        public async Task<Result<Employee>> GetEmployeeById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Result<Employee>.Failure("Employee ID is required.");
                }

                var response = await employeeRepository.GetById(id);
                if (!response.IsSuccess)
                {
                    return Result<Employee>.Failure(response.ErrorMessage);
                }
                return Result<Employee>.Success(response.Value);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to retrieve employee: {e.Message}");
            }
        }

        public async Task<Result<Employee?>> GetEmployee(ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<Employee>.Failure("User ID not found in token.");
                }

                var userResult = await userRepository.GetById(userId);
                if (!userResult.IsSuccess)
                {
                    return Result<Employee>.Failure(userResult.ErrorMessage);
                }

                if (string.IsNullOrEmpty(userResult.Value.EmployeeId))
                {
                    return Result<Employee>.Failure("No employee associated with this user.");
                }

                var employeeResult = await employeeRepository.GetById(userResult.Value.EmployeeId);
                if (!employeeResult.IsSuccess)
                {
                    return Result<Employee>.Failure(employeeResult.ErrorMessage);
                }

                return Result<Employee>.Success(employeeResult.Value);
            }
            catch (Exception e)
            {
                {
                    return Result<Employee?>.Failure($"Failed to retrieve employee profile: {e.Message}");
                }
            }
        }

        public async Task<Result<Employee>> UpdateEmployeeBasic(string id, EmployeeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email))
                {
                    return Result<Employee>.Failure("Employee ID, name, and email are required.");
                }

                var employeeResult = await GetEmployeeById(id);
                if (!employeeResult.IsSuccess)
                {
                    return Result<Employee>.Failure(employeeResult.ErrorMessage);
                }

                var employee = employeeResult.Value;
                employee.Name = request.Name;
                employee.Email = request.Email;
                employee.Designation = request.Designation;
                employee.Department = request.Department;
                employee.ContactNumber = request.ContactNumber;
                employee.UpdatedAt = DateTime.UtcNow;

                var updateResult = await employeeRepository.Update(employee);
                if (!updateResult.IsSuccess)
                {
                    return Result<Employee>.Failure(updateResult.ErrorMessage);
                }

                return Result<Employee>.Success(updateResult.Value);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to update employee: {e.Message}");
            }
        }

        public async Task<Result> DeleteEmployee(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Result.Failure("Employee ID is required.");
                }

                var result = await employeeRepository.SoftDelete(id);
                if (!result.IsSuccess)
                {
                    return Result.Failure(result.ErrorMessage);
                }

                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Failure($"Failed to delete employee: {e.Message}");
            }
        }

        public async Task<Result<Employee>> UpdateAddress(string id, AddressDto address)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || address == null)
                {
                    return Result<Employee>.Failure("Employee ID and address are required.");
                }

                var employeeResult = await employeeRepository.GetById(id);
                if (!employeeResult.IsSuccess)
                {
                    return Result<Employee>.Failure(employeeResult.ErrorMessage);
                }

                var employee = employeeResult.Value;
                employee.Address.Street = address.Street;
                employee.Address.City = address.City;
                employee.Address.State = address.State;
                employee.Address.Country = address.Country;
                employee.Address.PostalCode = address.PostalCode;
                employee.Address.EmployeeId = id;
                employee.UpdatedAt = DateTime.UtcNow;

                var updateResult = await employeeRepository.Update(employee);
                if (!updateResult.IsSuccess)
                {
                    return Result<Employee>.Failure(updateResult.ErrorMessage);
                }

                return Result<Employee>.Success(updateResult.Value);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to update address: {e.Message}");
            }
        }

        public async Task<Result<Employee>> UpdateEmploymentHistory(string id, List<EmploymentDto> histories)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || histories == null || !histories.Any())
                {
                    return Result<Employee>.Failure("Employee ID and employment history are required.");
                }

                var employeeResult = await GetEmployeeById(id);
                if (!employeeResult.IsSuccess) return Result<Employee>.Failure(employeeResult.ErrorMessage);

                var employee = employeeResult.Value;
                if (employee.Employments == null)
                {
                    employee.Employments = new List<EmploymentHistory>();
                }

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
                var updateResult = await employeeRepository.Update(employee);
                if (!updateResult.IsSuccess)
                {
                    return Result<Employee>.Failure(updateResult.ErrorMessage);
                }

                return Result<Employee>.Success(updateResult.Value);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to update employment history: {e.Message}");
            }
        }

        public async Task<Result<List<EmployeeSummaryDto>>> GetAllEmployeesBasic()
        {
            try
            {
                var result = await employeeRepository.GetAllBasics();
                if (!result.IsSuccess)
                {
                    return Result<List<EmployeeSummaryDto>>.Failure(result.ErrorMessage);
                }

                return Result<List<EmployeeSummaryDto>>.Success(result.Value);
            }
            catch (Exception e)
            {
                return Result<List<EmployeeSummaryDto>>.Failure($"Failed to retrieve employee summaries: {e.Message}");
            }
        }

        public async Task<Result<(List<EmployeeSummaryDto>, int totalCount)>> GetEmployeesPaged(
            int pageNumber = 1,
            int pageSize = 3,
            string sortBy = "name",
            bool ascending = true)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return Result<(List<EmployeeSummaryDto>, int)>.Failure("Invalid page number or page size.");
                }

                var result = await employeeRepository.GetPaged(pageNumber, pageSize, sortBy, ascending);
                if (!result.IsSuccess)
                {
                    return Result<(List<EmployeeSummaryDto>, int)>.Failure(result.ErrorMessage);
                }

                return Result<(List<EmployeeSummaryDto>, int)>.Success(result.Value);
            }
            catch (Exception e)
            {
                return Result<(List<EmployeeSummaryDto>, int)>.Failure($"Failed to retrieve paged employees: {e.Message}");
            }
        }
    }
}
