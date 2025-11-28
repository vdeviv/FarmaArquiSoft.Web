using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Http;

namespace FarmaArquiSoft.Web.Services
{
    public class UserApi
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _http = factory.CreateClient("usersApi");
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Configura los headers Authorization y X-Actor-Id usando
        /// el usuario actualmente logueado en la web.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void ApplyAuthHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("No hay HttpContext disponible.");

            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("El usuario no está autenticado.");

            // 1) Obtener el token JWT
            //   Preferimos claim "access_token", y si no está, usamos la cookie "AuthToken"
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

        // ================================
        //           MÉTODOS PÚBLICOS
        // ================================

        // LISTAR USUARIOS (GET /api/user)
        public async Task<List<UserListItemDto>> GetAllAsync()
        {
            ApplyAuthHeaders();

            var res = await _http.GetAsync("/api/user");

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Status {(int)res.StatusCode} ({res.StatusCode}). Body: {body}");
            }

            var list = await res.Content.ReadFromJsonAsync<List<UserListItemDto>>();
            return list ?? new List<UserListItemDto>();
        }

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            ApplyAuthHeaders();

            var res = await _http.GetAsync($"/api/user/{id}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Status {(int)res.StatusCode} ({res.StatusCode}) en GetById. Body: {body}");
            }

            var dto = await res.Content.ReadFromJsonAsync<UserDTO>();
            return dto;
        }

        public async Task<HttpResponseMessage> CreateAsync(UserDTO dto)
        {
            ApplyAuthHeaders();
            return await _http.PostAsJsonAsync("/api/user", dto);
        }

        public async Task<HttpResponseMessage> UpdateAsync(UserDTO dto)
        {
            ApplyAuthHeaders();
            return await _http.PutAsJsonAsync($"/api/user/{dto.id}", dto);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            ApplyAuthHeaders();
            return await _http.DeleteAsync($"/api/user/{id}");
        }
    }
}
