using LaundryApplication.Data;
using LaundryApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LaundryApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly LaundryDbContext _context;

        public AdminController(LaundryDbContext context)
        {
            _context = context;
        }

        // POST: api/Admin/AddService
        [HttpPost("AddService")]
        public async Task<ActionResult<Service>> AddService([FromBody] Service newService)
        {
            if (newService == null || string.IsNullOrEmpty(newService.ServiceName) || string.IsNullOrEmpty(newService.MaterialType) || newService.Price <= 0)
            {
                return BadRequest("Service name, material type, and price are required.");
            }

            // Check if the service already exists for this material type
            var existingService = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceName == newService.ServiceName && s.MaterialType == newService.MaterialType);

            if (existingService != null)
            {
                return Conflict("A service for this material type already exists.");
            }

            // Add the new service
            newService.Id = Guid.NewGuid();  // Assign a new unique ID
            _context.Services.Add(newService);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AddService), new { id = newService.Id }, newService);
        }

        // GET: api/Admin/Services
        [HttpGet("Services")]
        public async Task<ActionResult> GetAllServices()
        {
            var services = await _context.Services.ToListAsync();
            return Ok(services);
        }

        // GET: api/Admin/Services/{id}
        [HttpGet("Services/{id}")]
        public async Task<ActionResult<Service>> GetServiceById(Guid id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound("Service not found.");
            }
            return Ok(service);
        }
        // PUT: api/Admin/Services/{id}
        [HttpPut("Services/{id}")]
        public async Task<ActionResult<Service>> UpdateService(Guid id, [FromBody] Service updatedService)
        {
            if (updatedService == null || string.IsNullOrEmpty(updatedService.ServiceName) ||
                string.IsNullOrEmpty(updatedService.MaterialType) || updatedService.Price <= 0)
            {
                return BadRequest("Service name, material type, and price are required.");
            }

            var existingService = await _context.Services.FindAsync(id);
            if (existingService == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }

            // Check if any order exists for this service
            bool orderExists = await _context.Orders.AnyAsync(o => o.ServiceId == id);
            if (orderExists)
            {
                return Conflict("Cannot update service because orders exist for this service.");
            }

            // Check if another service with the same name & material type already exists
            var duplicateService = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceName == updatedService.ServiceName
                                       && s.MaterialType == updatedService.MaterialType
                                       && s.Id != id);
            if (duplicateService != null)
            {
                return Conflict("A service with the same name and material type already exists.");
            }

            // Update properties
            existingService.ServiceName = updatedService.ServiceName;
            existingService.MaterialType = updatedService.MaterialType;
            existingService.Price = updatedService.Price;

            _context.Entry(existingService).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(existingService);
        }
        // DELETE: api/Admin/Services/{id}
        [HttpDelete("Services/{id}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }

            // Check if any order exists for this service
            bool orderExists = await _context.Orders.AnyAsync(o => o.ServiceId == id);
            if (orderExists)
            {
                return Conflict("Cannot delete service because orders exist for this service.");
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 - Successfully deleted, no content to return
        }

        // GET: api/Admin/Orders
        [HttpGet("Orders")]
        public async Task<ActionResult<IEnumerable<GetOrdersDTO>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Join(_context.Services,
                    order => order.ServiceId,
                    service => service.Id,
                    (order, service) => new GetOrdersDTO
                    {
                        Id = order.Id,
                        CustomerId = order.CustomerId,
                        ServiceId = order.ServiceId,
                        ServiceName = service.ServiceName,
                        MaterialType = service.MaterialType,
                        Price = service.Price,
                        Quantity = order.Quantity,
                        ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                        AdditionalDescription = order.AdditionalDescription,
                        Status = order.Status,
                        DateCreated = order.DateCreated
                    })
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found.");
            }

            return Ok(orders);
        }

        // PUT: api/Admin/Orders/{id}/Status
        [HttpPut("Orders/{id}/Status")]
        public async Task<ActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            // Update the order status
            order.Status = status;
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();  // Status 204 - Successfully updated, no content to return
        }
    }
}
