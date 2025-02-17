using System.ComponentModel.DataAnnotations;

namespace LaundryApplication.Models
{
    public class Service
    {
        [Key]
        public Guid Id { get; set; }           // Unique identifier for the service
        public string ServiceName { get; set; } // Name of the service (e.g., "Ironing", "Dry Cleaning")
        public string MaterialType { get; set; } // Material type (e.g., "Cotton", "Silk")
        public decimal Price { get; set; }      // Price for the service based on the material type
    }
}
