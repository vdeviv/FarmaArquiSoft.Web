using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs;

namespace FarmaArquiSoft.Web.Services
{
    public class ProviderApi
    {
        private readonly HttpClient _http;

        public ProviderApi(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _http = factory.CreateClient("providersApi");

            var httpContext = httpContextAccessor.HttpContext;

            // 1) JWT desde cookie para Authorization
            var jwt = httpContext?.Request.Cookies["AuthToken"];
            if (!string.IsNullOrWhiteSpace(jwt))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", jwt);
            }

            // 2) X-Actor-Id desde cookie UserId
            var userIdCookie = httpContext?.Request.Cookies["UserId"];
            if (int.TryParse(userIdCookie, out var actorId))
            {
                if (_http.DefaultRequestHeaders.Contains("X-Actor-Id"))
                    _http.DefaultRequestHeaders.Remove("X-Actor-Id");

                _http.DefaultRequestHeaders.Add("X-Actor-Id", actorId.ToString());
            }
        }

        public async Task<List<ProviderDTO>> GetAllAsync()
        {
            var res = await _http.GetAsync("/api/Providers");
            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<List<ProviderDTO>>();
            return list ?? new List<ProviderDTO>();
        }

        public async Task<ProviderDTO?> GetByIdAsync(int id)
        {
            var res = await _http.GetAsync($"/api/Providers/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<ProviderDTO>();
        }

        public async Task<HttpResponseMessage> CreateAsync(ProviderDTO dto)
        {
            return await _http.PostAsJsonAsync("/api/Providers", dto);
        }

        public async Task<HttpResponseMessage> UpdateAsync(ProviderDTO dto)
        {
            return await _http.PutAsJsonAsync($"/api/Providers/{dto.id}", dto);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            return await _http.DeleteAsync($"/api/Providers/{id}");
        }
    }
}
