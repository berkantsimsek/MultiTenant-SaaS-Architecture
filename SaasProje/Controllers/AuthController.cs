using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens;
using SaaSProje.Data;
using SaaSProje.DTOs;
using SaaSProje.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaaSProje.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // 1. FİRMA VE KULLANICI KAYDI
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            // Önce yeni firmayı oluştur
            var newFirm = new Firm
            {
                Name = request.FirmName,
                IsActive = true
            };

            _context.Firms.Add(newFirm);
            _context.SaveChanges(); 

            // Sonra bu firmaya bağlı ilk kullanıcıyı oluştur
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = request.Password, 
                FirmId = newFirm.Id
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(new { Message = "Firma ve kullanıcı başarıyla oluşturuldu." });
        }

        // 2. GİRİŞ YAPMA VE TOKEN ÜRETME
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            // Kullanıcıyı bul
            var user = _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefault(u => u.Email == request.Email && u.PasswordHash == request.Password);

            if (user == null)
                return Unauthorized(new { Message = "Hatalı e-posta veya şifre!" });

            // Kullanıcı bulunduysa JWT oluştur
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirmId", user.FirmId.ToString()) 
            };

            
            var jwtKey = Encoding.ASCII.GetBytes("super_gizli_cok_uzun_jwt_anahtari_buraya_yazilmalidir_12345");
            var creds = new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Token 8 saat geçerlidir.
                signingCredentials: creds
            );

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Message = "Giriş başarılı!"
            });
        }
    }
}