using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Http;

namespace FarmaArquiSoft.Web.Services
{
    public class SaleDetailApi
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaleDetailApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            // Asegúrate que "SaleDetailsApi" esté registrado en Program.cs con el puerto correcto (5200)
            _http = factory.CreateClient("SaleDetailsApi");
            _httpContextAccessor = httpContextAccessor;
        }

        private void ApplyAuthHeaders()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Si no hay contexto (ej: llamadas background), no aplicamos auth, o logueamos warning
            if (httpContext == null) return;

            var user = httpContext.User;

            // 1) Obtener Token (Cookie o Claim)
            var token = user.FindFirst("access_token")?.Value;

            if (string.IsNullOrWhiteSpace(token))
            {
                httpContext.Request.Cookies.TryGetValue("AuthToken", out token);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Solo log para debug, no lanzamos excepción para permitir pruebas si quitaste [Authorize]
                Console.WriteLine(" SaleDetailApi: No se encontró token JWT. Petición anónima.");
            }

            // 2) Obtener ActorId (ID del usuario)
            var actorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Limpiar header anterior si existe
            if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
            {
                _http.DefaultRequestHeaders.Remove("X-Actor-Id");
            }

            if (!string.IsNullOrWhiteSpace(actorId))
            {
                _http.DefaultRequestHeaders.Add("X-Actor-Id", actorId);
            }
        }

        public async Task<List<SaleDetailDTO>> GetBySaleIdAsync(string saleId)
        {
            try
            {
                ApplyAuthHeaders();

                var url = $"api/SaleDetails/sale/{saleId}";
                // Console.WriteLine($"🌐 Llamando a: {_http.BaseAddress}{url}");

                var res = await _http.GetAsync(url);

                if (res.IsSuccessStatusCode)
                {
                    // Usamos opciones tolerantes a mayúsculas/minúsculas
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // Leemos el string primero por seguridad
                    var jsonString = await res.Content.ReadAsStringAsync();

                    // Si viene vacío "[]", retornamos lista vacía
                    if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "[]")
                        return new List<SaleDetailDTO>();

                    return JsonSerializer.Deserialize<List<SaleDetailDTO>>(jsonString, options) ?? new List<SaleDetailDTO>();
                }
                else
                {
                    // Loguear error real si falla (401, 500, etc)
                    Console.WriteLine($" Error SaleDetailApi: {res.StatusCode} - {await res.Content.ReadAsStringAsync()}");
                    return new List<SaleDetailDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Excepción en SaleDetailApi: {ex.Message}");
                return new List<SaleDetailDTO>();
            }
        }
    }
}