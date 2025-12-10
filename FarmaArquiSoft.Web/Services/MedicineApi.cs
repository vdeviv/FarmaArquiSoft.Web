using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Http;

namespace FarmaArquiSoft.Web.Services
{
    public class MedicineApi
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MedicineApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            // OJO: Asegúrate de registrar "medicinesApi" en Program.cs
            _http = factory.CreateClient("medicinesApi");
            _httpContextAccessor = httpContextAccessor;
        }

        private void ApplyAuthHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No hay HttpContext disponible.");

            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("El usuario no está autenticado.");

            // 1) Obtener Token
            var token = user.FindFirst("access_token")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                httpContext.Request.Cookies.TryGetValue("AuthToken", out token);
            }

            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("No se encontró token JWT.");

            // 2) Obtener Actor ID
            var actorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 3) Headers
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
                _http.DefaultRequestHeaders.Remove("X-Actor-Id");

            if (!string.IsNullOrWhiteSpace(actorId))
                _http.DefaultRequestHeaders.Add("X-Actor-Id", actorId);
        }

        public async Task<List<MedicineDTO>> GetAllAsync()
        {
            ApplyAuthHeaders();
            var res = await _http.GetAsync("/api/medicines");
            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<List<MedicineDTO>>();
            return list ?? new List<MedicineDTO>();
        }

        public async Task<MedicineDTO?> GetByIdAsync(int id)
        {
            ApplyAuthHeaders();
            var res = await _http.GetAsync($"/api/medicines/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<MedicineDTO>();
        }

        public async Task<HttpResponseMessage> CreateAsync(MedicineDTO dto)
        {
            ApplyAuthHeaders();
            return await _http.PostAsJsonAsync("/api/medicines", dto);
        }

        public async Task<HttpResponseMessage> UpdateAsync(MedicineDTO dto)
        {
            ApplyAuthHeaders();
            return await _http.PutAsJsonAsync($"/api/medicines/{dto.Id}", dto);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            ApplyAuthHeaders();
            return await _http.DeleteAsync($"/api/medicines/{id}");
        }
    }
}