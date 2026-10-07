using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SaaSProje.Data;
using SaaSProje.Models;
using System.Text.Json;

namespace SaaSProje.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache; // Redis için
        private readonly ElasticsearchClient _elasticClient; // Elasticsearch için

        public ProductsController(AppDbContext context, IDistributedCache cache, ElasticsearchClient elasticClient)
        {
            _context = context;
            _cache = cache;
            _elasticClient = elasticClient;
        }

        // --- YENİ: VERİ İZOLASYONU İÇİN FİRMA/KULLANICI KİMLİĞİ YAKALAYICI ---
        private string GetCurrentFirmId()
        {
            // JWT Token içerisinden firmanın veya kullanıcının benzersiz kimliğini güvenle alır
            return User.Claims.FirstOrDefault(c => c.Type.ToLower() == "firmid" || c.Type.ToLower() == "tenantid")?.Value
                   ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                   ?? "default_tenant";
        }

        // 1. ÜRÜNLERİ LİSTELEME
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            string firmId = GetCurrentFirmId();
            string cacheKey = $"all_products_{firmId}"; // REDIS İZOLASYONU

            // 1. Adım: Önce Redis'e sor (Sadece kendi firmasının önbelleğine bakar)
            var cachedProducts = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedProducts))
            {
                var productsFromCache = JsonSerializer.Deserialize<List<Product>>(cachedProducts);
                return Ok(productsFromCache);
            }

            // 2. Adım: SQL İZOLASYONU (.IgnoreQueryFilters() kaldırıldı!)
            // Artık Entity Framework arka planda sadece oturum açan firmanın ürünlerini süzecek.
            var productsFromDb = await _context.Products.ToListAsync();

            // 3. Adım: Sonucu SADECE BU FİRMAYA AİT Redis anahtarına kaydet
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(productsFromDb), cacheOptions);

            return Ok(productsFromDb);
        }

        // 2. ÜRÜN EKLEME
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] Product request)
        {
            string firmId = GetCurrentFirmId();

            // 1. SQL'e ekle
            _context.Products.Add(request);
            await _context.SaveChangesAsync();

            // 2. Sadece bu firmanın önbelleğini çöpe at
            await _cache.RemoveAsync($"all_products_{firmId}");

            // 3. ELASTICSEARCH İZOLASYONU: Ürünü sadece bu firmaya özel dizine kaydet
            string indexName = $"products_{firmId}".ToLower();
            var indexResponse = await _elasticClient.IndexAsync(request, indexName);

            return Ok(new { Message = "Ürün başarıyla SQL'e eklendi, Redis temizlendi ve Elastic'e indekslendi." });
        }

        // 3. ELASTICSEARCH İLE AKILLI ARAMA
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest("Lütfen aranacak bir kelime girin.");

            string firmId = GetCurrentFirmId();
            string indexName = $"products_{firmId}".ToLower();

            // Elastic'e git ve SADECE BU FİRMANIN DİZİNİNDE (indexName) arama yap
            var searchResponse = await _elasticClient.SearchAsync<Product>(s => s
                .Index(indexName)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh.Match(m => m
                                .Field(f => f.Name)
                                .Query(keyword)
                                .Fuzziness(new Fuzziness("AUTO"))
                            ),
                            sh => sh.Wildcard(w => w
                                .Field(f => f.Name)
                                .Value($"*{keyword.ToLower()}*")
                            )
                        )
                    )
                )
            );

            if (searchResponse.IsValidResponse)
            {
                return Ok(searchResponse.Documents.ToList());
            }

            return BadRequest(searchResponse.DebugInformation);
        }

        // 4. TEMİZLEME ARACI (SADECE İLGİLİ FİRMANIN VERİLERİNİ SIFIRLAR)
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAllProducts()
        {
            string firmId = GetCurrentFirmId();
            string indexName = $"products_{firmId}".ToLower();

            // 1. SQL: Sadece bu firmaya ait ürünleri sil
            var myProducts = await _context.Products.ToListAsync();
            _context.Products.RemoveRange(myProducts);
            await _context.SaveChangesAsync();

            // 2. Redis: Sadece bu firmaya ait önbelleği sil
            await _cache.RemoveAsync($"all_products_{firmId}");

            // 3. Elastic: Sadece bu firmaya ait arama dizinini tamamen uçur
            await _elasticClient.Indices.DeleteAsync(indexName);

            return Ok("Sadece sizin firmanıza ait tüm ürünler SQL, Redis ve Elasticsearch'ten başarıyla sıfırlandı!");
        }
    }
}