using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FarmaArquiSoft.Web.DTOs; 
using Microsoft.AspNetCore.Http;

namespace FarmaArquiSoft.Web.Services
{
    public class SaleApi
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaleApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {

            _http = factory.CreateClient("SalesApi");
            _httpContextAccessor = httpContextAccessor;
        }

        private void ApplyAuthHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No hay HttpContext disponible.");

            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("El usuario no está autenticado.");

            // 1) Obtener Token (Primero de Claims, si no, de Cookie)
            var token = user.FindFirst("access_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                httpContext.Request.Cookies.TryGetValue("AuthToken", out token);
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                // Log opcional para depurar en consola
                Console.WriteLine(" [SaleApi] ApplyAuthHeaders: No se encontró token.");
                throw new InvalidOperationException("No se encontró token JWT.");
            }

            // 2) Obtener Actor ID (Quién hace la venta)
            var actorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 3) Inyectar Headers
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
                _http.DefaultRequestHeaders.Remove("X-Actor-Id");

            if (!string.IsNullOrWhiteSpace(actorId))
                _http.DefaultRequestHeaders.Add("X-Actor-Id", actorId);
        }

        public async Task<HttpResponseMessage> CreateSaleAsync(CreateSaleRequest request)
        {
            ApplyAuthHeaders();
            // POST /api/Sales
            return await _http.PostAsJsonAsync("/api/Sales", request);
        }

        /// <summary>
        /// Obtiene el historial de ventas
        /// </summary>
        public async Task<List<SaleResponseDTO>> GetAllAsync()
        {
            ApplyAuthHeaders();

            // GET /api/Sales
            var res = await _http.GetAsync("/api/Sales");
            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<List<SaleResponseDTO>>();
            return list ?? new List<SaleResponseDTO>();
        }


        public async Task<SaleResponseDTO?> GetByIdAsync(string id)
        {
            ApplyAuthHeaders();

            // GET /api/Sales/{id}
            var res = await _http.GetAsync($"/api/Sales/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<SaleResponseDTO>();
        }
    }
}