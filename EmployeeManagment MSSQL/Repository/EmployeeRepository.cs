using EmployeeManagment.Dtos;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Data;
using EmployeeManagment_MSSQL.Exceptions;
using EmployeeManagment_MSSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagment.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Result<Employee>> CreateEmployee(Employee employee)
        {
            try
            {
                context.Employees.Add(employee);
                await context.SaveChangesAsync();
                return Result<Employee>.Success(employee);
            }
            catch (Exception e)
            {
               return Result<Employee>.Failure($"Failed to create employee: {e.Message}");
            }
        }

        public async Task<Result<Employee>> GetById(string id)
        {
            try
            {
                var employee =  await context.Employees.Include(e => e.Address)
                    .Include(e => e.Employments)
                    .FirstOrDefaultAsync(e => e.Id == id);
                if (employee == null)
                {
                    return Result<Employee>.Failure("Employee not found");
                }
                return Result<Employee>.Success(employee);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to retrive employee: {e.Message}");
            }
        }

        public async Task<Result<IEnumerable<Employee>>> GetAll()
        {
            try
            {
                var employees =  await context.Employees.Where(u =>  u.IsWorking == true).Include(e => e.Address)
                    .Include(e => e.Employments).OrderBy(e => e.Id).ToListAsync();
                return Result<IEnumerable<Employee>>.Success(employees);
            }
            catch (Exception e)
            {
                return Result<IEnumerable<Employee>>.Failure($"Failed to retrieve employees: {e.Message}");
            }
        }

        public async Task<Result<Employee>> Update(Employee employee)
        {
            try
            {
                context.Employees.Update( employee );
                await context.SaveChangesAsync();
                return Result<Employee>.Success(employee);
            }
            catch (Exception e)
            {
                return Result<Employee>.Failure($"Failed to update employee: {e.Message}");
            }
        }

        public async Task<Result> SoftDelete(string id)
        {
            try
            {
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == id);
                if (employee == null)
                {
                    return Result.Failure("Employee not found");
                }
                employee.IsWorking = false;
                await context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception e)
            {
               return Result.Failure($"Failed to delete employee: {e.Message}");
            }
        }

        public async Task<Result<List<EmployeeSummaryDto>>> GetAllBasics()
        {
            try
            {
                var employees =  await context.Employees.Where(u => u.IsWorking==true).Select(e => new EmployeeSummaryDto()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    Designation = e.Designation,
                    Department = e.Department,
                    ContactNumber = e.ContactNumber,
                }).ToListAsync();
                return Result<List<EmployeeSummaryDto>>.Success(employees);
            }
            catch (Exception e)
            {
               return Result<List<EmployeeSummaryDto>>.Failure($"Failed to retrieve employee summaries: {e.Message}");
            }
        }

        public async Task<Result<(List<EmployeeSummaryDto>, int totalCount)>> GetPaged(int pageNumber, int pageSize, string sortBy, bool ascending)
        {
            try
            {
                var fields = new HashSet<string> { "name", "department", "designation", "createdAt" };
                if (!fields.Contains(sortBy)) sortBy = "name";

                IQueryable<Employee> query = context.Employees;

                query = sortBy.ToLower() switch
                {
                    "department" => ascending
                        ? query.OrderBy(e => e.Department)
                        : query.OrderByDescending(e => e.Department),
                    "designation" => ascending
                        ? query.OrderBy(e => e.Designation)
                        : query.OrderByDescending(e => e.Designation),
                    "createdAt" => ascending ? query.OrderBy(e => e.CreatedAt) : query.OrderByDescending(e => e.CreatedAt),
                    _ => ascending ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
                };

                int totalCount = await query.CountAsync();

                var emp = await query.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new EmployeeSummaryDto()
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Email = e.Email,
                        Department = e.Department,
                        Designation = e.Designation,
                        ContactNumber = e.ContactNumber,
                    }).ToListAsync();
                return Result<(List<EmployeeSummaryDto>, int totalCount)>.Success((emp, totalCount));
            }
            catch (Exception e)
            {
               return Result<(List<EmployeeSummaryDto>, int totalCount)>.Failure($"Failed to retrieve paged employees: {e.Message}");
            }
        }
    }
}

