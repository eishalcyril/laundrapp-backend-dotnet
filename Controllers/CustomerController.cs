using LaundryApplication.Data;
using LaundryApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace LaundryApplication.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class CustomerController : ControllerBase
        {
            private readonly LaundryDbContext _context;

            public CustomerController(LaundryDbContext context)
            {
                _context = context;
            }

            // GET: api/Customer/Services
            [HttpGet("Services")]
            public async Task<ActionResult<IEnumerable<Service>>> GetAllServices()
            {
                var services = await _context.Services.ToListAsync();

                if (services == null || services.Count == 0)
                {
                    return NotFound("No services available.");
                }

                return Ok(services);
            }

            // GET: api/Customer/Services/{id}
            [HttpGet("Services/{id}")]
            public async Task<ActionResult<Service>> GetServiceById(Guid id)
            {
                var service = await _context.Services.FindAsync(id);

                if (service == null)
                {
                    return NotFound($"Service with ID {id} not found.");
                }

                return Ok(service);
            }

            // POST: api/Customer/PlaceOrder
            [HttpPost("PlaceOrder")]
            //[Authorize]  // Ensure that the user is authenticated
            public async Task<ActionResult<Order>> PlaceOrder([FromBody] Order orderRequest)
            {
                if (orderRequest == null || orderRequest.ServiceId == Guid.Empty || orderRequest.Quantity <= 0 || orderRequest.ExpectedDeliveryDate < DateTime.Now)
                {
                    return BadRequest("Service ID, quantity, expected delivery date, and additional description are required.");
                }

                // Get CustomerId from authenticated user (replace with your actual logic to get CustomerId)
                var customerId = GetCustomerIdFromUser(); // This should be your logic for extracting the logged-in user's ID

                // Ensure that the service ID is valid
                var service = await _context.Services.FindAsync(orderRequest.ServiceId);
                if (service == null)
                {
                    return NotFound($"Service with ID {orderRequest.ServiceId} not found.");
                }

                // Set the CustomerId to the logged-in user’s ID
                orderRequest.CustomerId = customerId;
                orderRequest.Id = Guid.NewGuid();  // Assign a new unique ID
                orderRequest.DateCreated = DateTime.UtcNow;  // Set the creation timestamp

                _context.Orders.Add(orderRequest);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(PlaceOrder), new { id = orderRequest.Id }, orderRequest);
            }

            // GET: api/Customer/Orders/{id}/Status
            [HttpGet("Orders/{id}/Status")]
            //[Authorize]  // Ensure that the user is authenticated
            public async Task<ActionResult> GetOrderStatus(Guid id)
            {
                var customerId = GetCustomerIdFromUser(); // This should be your actual method to get the authenticated user's CustomerId

                // Retrieve the order with the given ID and ensure the order belongs to the logged-in customer
                var order = await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found for this customer.");
                }
                return Ok(new { OrderId = order.Id, Status = order.Status });
            }

        // GET: api/Customer/Orders
        [HttpGet("Orders")]
        public async Task<ActionResult<IEnumerable<GetOrdersDTO>>> GetOrdersForCustomer()
        {
            var customerId = GetCustomerIdFromUser();

            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
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
                return NotFound("No orders found for this customer.");
            }

            return Ok(orders);
        }


        // GET: api/Customer/Orders/{id}
        [HttpGet("Orders/{id}")]
            [Authorize]  // Ensure that the user is authenticated
            public async Task<ActionResult<Order>> GetOrderById(Guid id)
            {
                var customerId = GetCustomerIdFromUser(); // Replace with actual method to get CustomerId

                // Ensure the customer only sees their own orders
                var order = await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found for this customer.");
                }

                return Ok(order);
            }

            [HttpPost("Orders/{id}/Cancel")]
            public async Task<ActionResult> CancelOrder(Guid id)
            {
                var customerId = GetCustomerIdFromUser(); // Get the logged-in user's CustomerId

                // Retrieve the order with the given ID and ensure the order belongs to the logged-in customer
                var order = await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found for this customer.");
                }

                // Check if the order can be cancelled (e.g., status must not be "Completed" or "Cancelled")
                if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Completed)
                {
                    return BadRequest("This order cannot be cancelled because it is already completed or cancelled.");
                }

                // Update the order status to "Cancelled"
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Order successfully cancelled." });
            }
            [HttpPut("Orders/{id}")]
            [Authorize]  // Ensure that the user is authenticated
            public async Task<ActionResult> EditOrder(Guid id, [FromBody] Order updatedOrder)
            {
                var customerId = GetCustomerIdFromUser(); // Get the logged-in user's CustomerId

                // Retrieve the order with the given ID and ensure the order belongs to the logged-in customer
                var order = await _context.Orders
                    .Where(o => o.CustomerId == customerId)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound($"Order with ID {id} not found for this customer.");
                }

                // Check if the order can be edited (it must not be completed or cancelled)
                if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
                {
                    return BadRequest("This order cannot be edited because it is either completed or cancelled.");
                }

                // Update the order fields with the new values from the request
                order.Quantity = updatedOrder.Quantity > 0 ? updatedOrder.Quantity : order.Quantity;
                order.ExpectedDeliveryDate = updatedOrder.ExpectedDeliveryDate != DateTime.MinValue ? updatedOrder.ExpectedDeliveryDate : order.ExpectedDeliveryDate;
                order.AdditionalDescription = !string.IsNullOrEmpty(updatedOrder.AdditionalDescription) ? updatedOrder.AdditionalDescription : order.AdditionalDescription;

                // Save the updated order
                await _context.SaveChangesAsync();

                return Ok(order); // Return the updated order
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
