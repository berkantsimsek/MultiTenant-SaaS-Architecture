using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSMvcClient.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SaaSMvcClient.Controllers
{
    [Authorize(AuthenticationSchemes = "CookieAuth")] 
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ---------------- YENİ EKLENEN VİTRİN BÖLÜMÜ ----------------
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Kullanıcının (Müşterinin) yetki token'ını al
            var token = User.FindFirst("jwt_token")?.Value;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. API'den SADECE bu firmanın ürünlerini iste
            var response = await _httpClient.GetAsync("https://localhost:7061/api/Products");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                // 3. Gelen JSON verisini C# listesine çevir
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var products = JsonSerializer.Deserialize<List<ProductViewModel>>(responseString, options);

                return View(products);
            }

            // Hata olursa veya ürün yoksa boş liste gönder
            return View(new List<ProductViewModel>());
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Kullanıcı "Kaydet" butonuna bastığında çalışacak kod
        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            // 1. Giriş yapan kullanıcının token'ını al ve API'ye tanıt
            var token = User.FindFirst("jwt_token")?.Value;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Formdan gelen veriyi JSON'a çevir (API'nin anladığı dil)
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            // 3. API'nin Products ucuna POST isteği at
            var response = await _httpClient.PostAsync("https://localhost:7061/api/Products", content);

            if (response.IsSuccessStatusCode)
            {
                // İşlem başarılıysa ekranda yeşil bir mesaj göster
                TempData["Message"] = $"{model.Name} ürünü mağazanıza başarıyla eklendi!";
                return RedirectToAction("Create");
            }

            ViewBag.Error = "Ürün eklenirken bir hata oluştu.";
            return View(model);
        }
    }
}