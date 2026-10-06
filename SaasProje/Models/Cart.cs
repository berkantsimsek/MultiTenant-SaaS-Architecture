namespace SaaSProje.Models
{
    public class Cart : IMustHaveTenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } 
        public Guid ProductId { get; set; } 
        public int Quantity { get; set; } 
        public Guid FirmId { get; set; } 
    }
}