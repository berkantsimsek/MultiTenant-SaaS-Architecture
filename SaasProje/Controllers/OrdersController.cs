using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSProje.Data;
using SaaSProje.Models;

namespace SaaSProje.Controllers
{
    [Authorize] // Sadece giriş yapan firmalar erişebilir
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            // Global Query Filter (Tenant mimarisi) sayesinde, 
            // _context.Orders sadece token'ı gönderen firmaya ait siparişleri getirir.
            var orders = _context.Orders.ToList();
            return Ok(orders);
        }

        [HttpPost("checkout/{userId}")]
        public IActionResult Checkout(Guid userId)
        {
            // 1. Müşterinin sepetindeki ürünleri bul
            var cartItems = _context.Carts.Where(c => c.UserId == userId).ToList();

            if (!cartItems.Any())
                return BadRequest(new { Message = "Sepetiniz boş, sipariş oluşturulamaz." });

            // 2. Yeni bir Ana Sipariş (Order) oluştur
            var order = new Order
            {
                UserId = userId,
                TotalAmount = 0 
            };
            _context.Orders.Add(order);

            decimal totalAmount = 0;

            // 3. Sepetteki her bir ürünü Sipariş Kalemine (OrderItem) çevir
            foreach (var cartItem in cartItems)
            {
                // Ürünün güncel fiyatını veritabanından çek
                var product = _context.Products.Find(cartItem.ProductId);
                var unitPrice = product != null ? product.Price : 0;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                };

                _context.OrderItems.Add(orderItem);
                totalAmount += (unitPrice * cartItem.Quantity);
            }

            order.TotalAmount = totalAmount;

            // 4. Sipariş başarıyla oluşturulduğuna göre müşterinin sepetini temizle
            _context.Carts.RemoveRange(cartItems);

            // 5. Tüm bu işlemleri veritabanına tek seferde kaydet
            _context.SaveChanges();

            return Ok(new { Message = "Siparişiniz başarıyla alındı.", OrderNumber = order.OrderNumber, TotalAmount = totalAmount });
        }
    }
}