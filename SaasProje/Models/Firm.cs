namespace SaaSProje.Models
{
    public class Firm
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // 36 karakterli eşsiz kimlik
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}