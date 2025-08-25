using EmployeeManagment.Dtos;
using EmployeeManagment.Interfaces;
using EmployeeManagment.Models;
using EmployeeManagment_MSSQL.Dtos;
using EmployeeManagment_MSSQL.Exceptions;
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
            if (newEmployee.IsSuccess)
            {
                return Ok(newEmployee.Value);
            }
            return BadRequest(new { Error = newEmployee.ErrorMessage });
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
            var result = await employeeService.GetEmployeeById(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Error = result.ErrorMessage });
            }
            return Ok(result.Value);
        }
        
        [HttpGet("me")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await employeeService.GetEmployee(User);
            if (!result.IsSuccess) return NotFound(new { Error = result.ErrorMessage });

            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmployee(string id, [FromBody] EmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await employeeService.UpdateEmployeeBasic(id, request);
            if (!result.IsSuccess) return NotFound(new { Error = result.ErrorMessage });

            return Ok(result.Value);
        }
        
        [HttpPut("{id}/address")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAddress(string id, [FromBody] AddressDto address)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await employeeService.UpdateAddress(id, address);
            if (!result.IsSuccess) return NotFound(new { Error = result.ErrorMessage });

            return Ok(result.Value);
        }
        
        [HttpPut("{id}/history")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEmploymentHistory(string id, [FromBody] List<EmploymentDto> history)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await employeeService.UpdateEmploymentHistory(id, history);
            if (!result.IsSuccess) return NotFound(new { Error = result.ErrorMessage });

            return Ok(result.Value);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var result = await employeeService.DeleteEmployee(id);
            if (!result.IsSuccess) return NotFound(new { Error = result.ErrorMessage });

            return Ok(new { message = "Employee deleted successfully" });
        }

        [HttpGet("basic")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployeesBasic()
        {
            var result = await employeeService.GetAllEmployeesBasic();
            if(!result.IsSuccess) { return BadRequest(new { Error = result.ErrorMessage }); }
            return Ok(result.Value);
        }

        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEmployeesPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 3,
            [FromQuery] string sortBy = "username",
            [FromQuery] bool ascending = true)
        {
            var result = await employeeService.GetEmployeesPaged(pageNumber, pageSize, sortBy, ascending);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Error = result.ErrorMessage });

            }

            return Ok(new
            {
                Data = result.Value.Item1,
                totalCount = result.Value.Item2
            });
        }

    }
}
