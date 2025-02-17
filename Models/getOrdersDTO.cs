namespace LaundryApplication.Models
{



    public class GetOrdersDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; } // Link the order to a customer
        public Guid ServiceId { get; set; }
        //public Service service { get; set; }
        public string ServiceName { get; set; } // Name of the service (e.g., "Ironing", "Dry Cleaning")
        public string MaterialType { get; set; } // Material type (e.g., "Cotton", "Silk")
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public string AdditionalDescription { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}

