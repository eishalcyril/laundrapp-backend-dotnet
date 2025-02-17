namespace LaundryApplication.Models
{
    public class UserSignupDTO
    {
        public string FirstName { get; set; }     // First name of the user
        public string LastName { get; set; }      // Last name of the user

        public string Email { get; set; }         // User's email address
        public string Address { get; set; }       // User's address
        public string PhoneNumber { get; set; }

        public string Password { get; set; }      // User's password
    }
}
