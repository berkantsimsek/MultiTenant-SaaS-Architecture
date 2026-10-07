using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SaaSMvcClient.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SaaSMvcClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            
            var loginData = new { email = model.Email, password = model.Password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

           
            var response = await _httpClient.PostAsync("https://localhost:7061/api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                
                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseString);
                var token = result.GetProperty("token").GetString();

                var claims = new List<Claim> { new Claim("jwt_token", token) };
                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

               
                return RedirectToAction("Index", "Orders");
            }

            ViewBag.Error = "Giriş başarısız! Lütfen bilgilerinizi kontrol edin.";
            return View(model);
        }

        
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
           
            await HttpContext.SignOutAsync("CookieAuth");

            
            return RedirectToAction("Login");
        }
    }
}