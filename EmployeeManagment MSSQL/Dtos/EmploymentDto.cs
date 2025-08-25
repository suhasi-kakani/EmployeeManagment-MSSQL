namespace EmployeeManagment_MSSQL.Dtos
{
    public class EmploymentDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime startDate { get; set; }
        public DateTime? endDate { get; set; }
    }
}
