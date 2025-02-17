using LaundryApplication.Data;
using LaundryApplication.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LaundryApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly LaundryDbContext _context;

        public UserController(LaundryDbContext context)
        {
            _context = context;
        }

        // POST: api/User/Signup
        [HttpPost("Signup")]
        public async Task<ActionResult<User>> Signup([FromBody] UserSignupDTO signupRequest)
        {
            if (string.IsNullOrEmpty(signupRequest.Email) || string.IsNullOrEmpty(signupRequest.Password))
            {
                return BadRequest("Email and password are required.");
            }

            // Check if the user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == signupRequest.Email);
            if (existingUser != null)
            {
                return Conflict("A user with this email already exists.");
            }

            // Create a new User object and map properties from the DTO
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = signupRequest.FirstName,
                LastName = signupRequest.LastName,
                Email = signupRequest.Email,
                PhoneNumber = signupRequest.PhoneNumber, // Not provided in the signup request
                Address = signupRequest.Address,
                IsAdmin = false, // Automatically set the user as a customer
                PasswordHash = HashPassword(signupRequest.Password),
                DateCreated = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Signup), new { id = newUser.Id }, newUser);
        }

        // POST: api/User/Login
        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDTO loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Email and password are required.");
            }

            // Find the user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Verify the password hash
            if (!VerifyPassword(loginRequest.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Return the user data excluding the password
            user.PasswordHash = null;
            return Ok(user);
        }

        // Helper function to hash a password
        private string HashPassword(string password)
        {
            // Salt is a random value added to the password before hashing to prevent rainbow table attacks.
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashedPassword}";
        }

        // Helper function to verify the password hash
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            var parts = storedPasswordHash.Split('.');
            byte[] salt = Convert.FromBase64String(parts[0]);
            string storedHash = parts[1];

            string hashedEnteredPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return storedHash == hashedEnteredPassword;
        }
        // PATCH: api/User/UpdateAddress
        [HttpPatch("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressDTO addressUpdate)
        {
            if (string.IsNullOrWhiteSpace(addressUpdate.NewAddress))
            {
                return BadRequest("Address cannot be empty.");
            }

            try
            {
                Guid customerId = GetCustomerIdFromUser(); // Fetch Customer ID from headers

                var user = await _context.Users.FindAsync(customerId);           
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.Address = addressUpdate.NewAddress;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Address updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }




        private Guid GetCustomerIdFromUser()
        {
            // Get the CustomerId from the request header (set by frontend)
            var customerIdString = Request.Headers["CustomerId"].FirstOrDefault();

            if (!string.IsNullOrEmpty(customerIdString) && Guid.TryParse(customerIdString, out Guid customerId))
            {
                return customerId;
            }

            throw new UnauthorizedAccessException("Customer ID not found in the request header.");
        }
    }
}

