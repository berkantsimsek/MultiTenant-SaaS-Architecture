namespace SaaSProje.Models
{
    public class Product : IMustHaveTenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public Guid FirmId { get; set; } 
    }
}