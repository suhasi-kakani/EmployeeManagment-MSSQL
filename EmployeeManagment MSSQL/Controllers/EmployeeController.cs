using EmployeeManagment.Dtos;
using EmployeeManagment.Interfaces;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeRequest employee)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var newEmployee = await employeeService.CreateEmployee(employee);
            return Ok(newEmployee);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await employeeService.GetEmployees();
            return Ok(employees);
        }

        [HttpGet("emp/{id}")]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            var employee = await employeeService.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound("No employee found");
            }
            return Ok(employee);
        }
        
        [HttpGet("me")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyProfile()
        {

            var employee = await employeeService.GetEmployee(User);
            if (employee == null) return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] EmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await employeeService.UpdateEmployeeBasic(id, request);
            if (employee == null) return NotFound();

            return Ok(employee);
        }
        
        [HttpPut("{id}/address")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAddress(string id, [FromBody] AddressDto address)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await employeeService.UpdateAddress(id, address);
            if (employee == null) return NotFound();

            return Ok(employee);
        }
        
        [HttpPut("{id}/history")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmploymentHistory(string id, [FromBody] List<EmploymentDto> history)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await employeeService.UpdateEmploymentHistory(id, history);
            if (employee == null) return NotFound();

            return Ok(employee);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var deleted = await employeeService.DeleteEmployee(id);
            if (!deleted) return NotFound();

            return Ok(new { message = "Employee deleted successfully" });
        }

        [HttpGet("basic")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployeesBasic()
        {
            var employees = await employeeService.GetAllEmployeesBasic();
            return Ok(employees);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployeesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 3,
            [FromQuery] string sortBy = "name",
            [FromQuery] bool ascending = true)
        {
            var (employees, totalCount) = await employeeService.GetEmployeesPaged(pageNumber, pageSize, sortBy, ascending);

            return Ok(new
            {
                Data = employees,
                totalCount = totalCount
            });
        }

    }
}
