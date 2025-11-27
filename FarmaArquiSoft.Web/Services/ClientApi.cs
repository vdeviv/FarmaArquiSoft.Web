using System.Net.Http.Json;
using System.Text.Json;
using FarmaArquiSoft.Web.DTOs;

namespace FarmaArquiSoft.Web.Services
{
    public class ClientApi
    {
        private readonly HttpClient _http;

        public ClientApi(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("clientsApi");
        }
        public async Task<List<ClientDTO>> GetAllAsync()
        {
            var res = await _http.GetAsync("/api/Clients");

            res.EnsureSuccessStatusCode();

            var list = await res.Content.ReadFromJsonAsync<List<ClientDTO>>();
            return list ?? new List<ClientDTO>();
        }
        public async Task<ClientDTO?> GetByIdAsync(int id)
        {
            var res = await _http.GetAsync($"/api/Clients/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<ClientDTO>();
        }
        public async Task<HttpResponseMessage> CreateAsync(ClientDTO dto)
        {
            return await _http.PostAsJsonAsync("/api/Clients", dto);
        }
        public async Task<HttpResponseMessage> UpdateAsync(ClientDTO dto)
        {
            return await _http.PutAsJsonAsync($"/api/Clients/{dto.id}", dto);
        }
        public async Task<HttpResponseMessage> DeleteAsync(int id)
        {
            return await _http.DeleteAsync($"/api/Clients/{id}");
        }
    }
}
