

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EmployeeManagment.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        [ForeignKey("EmployeeId")]
        public string EmployeeId { get; set; }
        [JsonIgnore]
        public Employee Employee { get; set; }
    }
}
