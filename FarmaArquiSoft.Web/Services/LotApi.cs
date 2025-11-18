using System.Net.Http.Json;
using FarmaArquiSoft.Web.DTOs;

namespace FarmaArquiSoft.Web.Services
{
    public class LotApi
    {
        private readonly HttpClient _http;

        public LotApi(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("lotesApi");
        }

        // GET ALL
        public async Task<List<LotDTO>> GetAll()
        {
            return await _http.GetFromJsonAsync<List<LotDTO>>("api/lots");
        }

        // GET BY ID
        public async Task<LotDTO?> GetById(int id)
        {
            return await _http.GetFromJsonAsync<LotDTO>($"api/lots/{id}");
        }

        // CREATE
        public async Task<bool> Create(LotDTO lot)
        {
            var response = await _http.PostAsJsonAsync("api/lots", lot);
            return response.IsSuccessStatusCode;
        }

        // UPDATE
        public async Task<bool> Update(int id, LotDTO lot)
        {
            var response = await _http.PutAsJsonAsync($"api/lots/{id}", lot);
            return response.IsSuccessStatusCode;
        }

        // DELETE
        public async Task<bool> Delete(int id)
        {
            var response = await _http.DeleteAsync($"api/lots/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
