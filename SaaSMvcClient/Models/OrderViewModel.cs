namespace SaaSMvcClient.Models
{
    public class OrderViewModel
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}