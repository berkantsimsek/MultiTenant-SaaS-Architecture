using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSMvcClient.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SaaSMvcClient.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")] 
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrdersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Kullanıcı giriş yaptığında Cookie'ye sakladığımız JWT Token'ı okuyoruz
            var token = User.FindFirst("jwt_token")?.Value;

            // 2. HttpClient'a bu Token'ı veriyoruz ki API bizi tanısın 
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 3. API'den firmanın siparişlerini çekiyoruz 
            var response = await _httpClient.GetAsync("https://localhost:7061/api/Orders");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // Gelen JSON verisini C# listesine çevir
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var orders = JsonSerializer.Deserialize<List<OrderViewModel>>(responseString, options);

                return View(orders);
            }

            return View(new List<OrderViewModel>());
        }
    }
}