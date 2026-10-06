using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using SaaSProje.Data;
using SaaSProje.Models;

namespace SaaSProje.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
           
            var products = _context.Products.IgnoreQueryFilters().ToList();
            return Ok(products);
        }

        [HttpPost]
        public IActionResult AddProduct([FromBody] Product request)
        {
            
            _context.Products.Add(request);
            _context.SaveChanges();

            return Ok(new { Message = "Ürün başarıyla eklendi." });
        }
    }
}