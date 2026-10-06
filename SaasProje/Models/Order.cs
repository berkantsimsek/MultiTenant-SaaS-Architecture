using System;
using System.Collections.Generic;

namespace SaaSProje.Models
{
    public class Order : IMustHaveTenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OrderNumber { get; set; } = "ORD-" + new Random().Next(10000, 99999);
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public Guid FirmId { get; set; }

        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}