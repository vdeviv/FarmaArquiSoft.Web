using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Http;

namespace FarmaArquiSoft.Web.Services
{
    public class LotApi
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LotApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _http = factory.CreateClient("lotesApi");
            _httpContextAccessor = httpContextAccessor;
        }
        private void ApplyAuthHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No hay HttpContext disponible.");

            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("El usuario no está autenticado.");

            //    Primero buscamos claim "access_token", y si no está, usamos la cookie "AuthToken"
            var token = user.FindFirst("access_token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                if (!httpContext.Request.Cookies.TryGetValue("AuthToken", out token) ||
                    string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("No se encontró el token JWT (claim ni cookie).");
                }
            }

            // 2) Obtener el actorId (id del usuario autenticado)
            var actorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 3) Configurar los headers del HttpClient para ESTA instancia
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
                _http.DefaultRequestHeaders.Remove("X-Actor-Id");

            if (!string.IsNullOrWhiteSpace(actorId))
                _http.DefaultRequestHeaders.Add("X-Actor-Id", actorId);
        }

        public async Task<List<LotDTO>> GetAllAsync()
        {
            ApplyAuthHeaders();

            var res = await _http.GetAsync("api/lots");
            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<List<LotDTO>>();
            return list ?? new List<LotDTO>();
        }

        public async Task<LotDTO?> GetByIdAsync(int id)
        {
            ApplyAuthHeaders();

            var res = await _http.GetAsync($"api/lots/{id}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<LotDTO>();
        }

        public Task<HttpResponseMessage> CreateAsync(LotDTO lot)
        {
            ApplyAuthHeaders();
            return _http.PostAsJsonAsync("api/lots", lot);
        }

        // Actualizar lote
        public Task<HttpResponseMessage> UpdateAsync(LotDTO lot)
        {
            ApplyAuthHeaders();
            return _http.PutAsJsonAsync($"api/lots/{lot.id}", lot);
        }

        // Eliminar lote
        public Task<HttpResponseMessage> DeleteAsync(int id)
        {
            ApplyAuthHeaders();
            return _http.DeleteAsync($"api/lots/{id}");
        }
        public async Task<List<LotDTO>> GetByMedicineAsync(int medicineId)
        {
            ApplyAuthHeaders();
            // Llama al nuevo endpoint que creamos arriba
            var res = await _http.GetAsync($"api/lots/medicine/{medicineId}");

            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<List<LotDTO>>() ?? new List<LotDTO>();
            }

            return new List<LotDTO>();
        }
    }
}
