namespace LaundryApplication.Models
{
    public enum OrderStatus
    {
        Pending,
        InProgress,
        Completed,
        Delivered,
        Cancelled
    }


        public class Order
        {
            public Guid Id { get; set; }
            public Guid CustomerId { get; set; } // Link the order to a customer
            public Guid ServiceId { get; set; }
            public int Quantity { get; set; }
            public DateTime ExpectedDeliveryDate { get; set; }
            public string AdditionalDescription { get; set; }
            public OrderStatus Status { get; set; }
            public DateTime DateCreated { get; set; }
        }
    }

