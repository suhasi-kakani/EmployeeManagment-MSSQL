
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EmployeeManagment.Models
{
    public class EmploymentHistory
    {
        [Key]
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime startDate { get; set; }
        public DateTime? endDate { get; set; }

        [ForeignKey("EmployeeId")]
        public string EmployeeId { get; set; }
        [JsonIgnore]
        public Employee Employee { get; set; }
    }
}
