using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSProje.Data;
using SaaSProje.Models;

namespace SaaSProje.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartsController(AppDbContext context)
        {
            _context = context;
        }

        // Sepete ürün ekleme metodu
        [HttpPost]
        public IActionResult AddToCart([FromBody] Cart request)
        {
            // request içinden id ve firmId göndermenize gerek yok, 
            // Guid otomatik oluşacak, FirmId ise CurrentTenantService'den alınacak.
            _context.Carts.Add(request);
            _context.SaveChanges();

            return Ok(new { Message = "Ürün başarıyla sepete eklendi." });
        }

        // Sepetteki ürünleri görme metodu Test amaçlı yaptım
        [HttpGet("{userId}")]
        public IActionResult GetCart(Guid userId)
        {
            var cartItems = _context.Carts.Where(c => c.UserId == userId).ToList();
            return Ok(cartItems);
        }
    }
}