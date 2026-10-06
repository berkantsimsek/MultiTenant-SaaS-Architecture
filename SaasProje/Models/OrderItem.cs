using System;

namespace SaaSProje.Models
{
    public class OrderItem : IMustHaveTenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; } 
        public Guid ProductId { get; set; } 
        public int Quantity { get; set; } 
        public decimal UnitPrice { get; set; } 
        public Guid FirmId { get; set; } 
    }
}