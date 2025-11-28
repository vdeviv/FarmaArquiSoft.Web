using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Http;

namespace FarmaArquiSoft.Web.Services
{
    public class ClientApi
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _http = factory.CreateClient("clientsApi");
            _httpContextAccessor = httpContextAccessor;
        }

        private void ApplyAuthHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No hay HttpContext disponible.");

            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("El usuario no está autenticado.");

            //    Preferimos claim "access_token", y si no está, usamos la cookie "AuthToken"
            var token = user.FindFirst("access_token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                if (!httpContext.Request.Cookies.TryGetValue("AuthToken", out token) ||
                    string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("No se encontró el token JWT (claim ni cookie).");
                }
            }


            var actorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
                _http.DefaultRequestHeaders.Remove("X-Actor-Id");

            if (!string.IsNullOrWhiteSpace(actorId))
                _http.DefaultRequestHeaders.Add("X-Actor-Id", actorId);
        }


        public async Task<List<ClientDTO>> GetAllAsync()
        {
            ApplyAuthHeaders();

            var res = await _http.GetAsync("/api/Clients");
            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<List<ClientDTO>>();
            return list ?? new List<ClientDTO>();
        }

        public async Task<ClientDTO?> GetByIdAsync(int id)
        {
            ApplyAuthHeaders();

            var res = await _http.GetAsync($"/api/Clients/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<ClientDTO>();
        }

        public async Task<HttpResponseMessage> CreateAsync(ClientDTO dto)
        {
            ApplyAuthHeaders();
            return await _http.PostAsJsonAsync("/api/Clients", dto);
        }

        public async Task<HttpResponseMessage> UpdateAsync(ClientDTO dto)
        {
            ApplyAuthHeaders();
            return await _http.PutAsJsonAsync($"/api/Clients/{dto.id}", dto);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            ApplyAuthHeaders();
            return await _http.DeleteAsync($"/api/Clients/{id}");
        }
    }
}
