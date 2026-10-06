using System;

namespace SaaSMvcClient.Models
{
    public class ProductViewModel
    {
        public Guid Id { get; set; } 
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}