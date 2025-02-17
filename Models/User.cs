using System.ComponentModel.DataAnnotations;

namespace LaundryApplication.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }               // Unique identifier for the user

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }     // First name of the user

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }      // Last name of the user

        [Required]
        [EmailAddress]
        public string Email { get; set; }         // User's email address

        [Phone]
        public string PhoneNumber { get; set; }   // User's phone number

        [Required]
        [StringLength(200)]
        public string Address { get; set; }
        public bool IsAdmin { get; set; }      

        public string PasswordHash { get; set; }
        public DateTime DateCreated { get; set; } 
    }
}
