using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs;

namespace FarmaArquiSoft.Web.Services
{
    public class UserApi
    {
        private readonly HttpClient _http;

        // 🔐 CREDENCIALES PARA /api/user/authenticate
        // Usa el usuario REAL de tu sistema (el que probaste en Postman)
        private const string AuthUsername = "adminRoot";
        private const string AuthPassword = "AdminRoot$2025!";

        // Cache SOLO a nivel de instancia (por request, porque UserApi es Scoped)
        private string? _jwtToken;
        private int? _actorId;
        private bool _authDone = false;

        public UserApi(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("usersApi");
        }

        // ================================
        //   MÉTODO PRIVADO: Asegurar JWT
        // ================================
        private async Task EnsureTokenAsync()
        {
            // Si ya nos autenticamos en esta instancia, no volvemos a hacerlo
            if (_authDone && !string.IsNullOrEmpty(_jwtToken))
                return;

            // 1) Llamar al endpoint de autenticación
            var loginBody = new
            {
                Username = AuthUsername,
                Password = AuthPassword
            };

            var res = await _http.PostAsJsonAsync("/api/user/authenticate", loginBody);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"No se pudo autenticar contra /api/user/authenticate. " +
                    $"Status: {(int)res.StatusCode} {res.ReasonPhrase}. Respuesta: {body}");
            }

            // 2) Leer JSON: { "token": "...", "user": { "id": 1, ... } }
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("token", out var tokenProp) ||
                tokenProp.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    "La respuesta de /api/user/authenticate no contenía un campo 'token' válido.");
            }

            var token = tokenProp.GetString();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException(
                    "El token devuelto por /api/user/authenticate está vacío.");
            }

            _jwtToken = token;

            // Obtener id de usuario autenticado para X-Actor-Id (si existe)
            if (root.TryGetProperty("user", out var userElement) &&
                userElement.ValueKind == JsonValueKind.Object &&
                userElement.TryGetProperty("id", out var idElement) &&
                idElement.TryGetInt32(out var parsedId))
            {
                _actorId = parsedId;
            }

            // 3) Configurar headers por defecto para ESTE HttpClient
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _jwtToken);

            if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
            {
                _http.DefaultRequestHeaders.Remove("X-Actor-Id");
            }

            if (_actorId.HasValue)
            {
                _http.DefaultRequestHeaders.Add("X-Actor-Id", _actorId.Value.ToString());
            }

            _authDone = true;
        }

        // ================================
        //           MÉTODOS PÚBLICOS
        // ================================

        // LISTAR USUARIOS (GET /api/user)
        public async Task<List<UserListItemDto>> GetAllAsync()
        {
            await EnsureTokenAsync();

            var res = await _http.GetAsync("/api/user");

            // Si hay 401, lanzamos con más detalle
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
            await EnsureTokenAsync();

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
            await EnsureTokenAsync();
            return await _http.PostAsJsonAsync("/api/user", dto);
        }

        public async Task<HttpResponseMessage> UpdateAsync(UserDTO dto)
        {
            await EnsureTokenAsync();
            return await _http.PutAsJsonAsync($"/api/user/{dto.id}", dto);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            await EnsureTokenAsync();
            return await _http.DeleteAsync($"/api/user/{id}");
        }
    }
}