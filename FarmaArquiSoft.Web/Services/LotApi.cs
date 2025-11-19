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

        public async Task<List<LotDTO>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<LotDTO>>("api/lots");
        }


        public async Task<LotDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<LotDTO>($"api/lots/{id}");
        }

        // Crear un nuevo lote
        public async Task CreateAsync(LotDTO lot)
        {
            var response = await _http.PostAsJsonAsync("api/lots", lot);

            if (!response.IsSuccessStatusCode)
                throw new Exception("No se pudo crear el lote.");
        }

        public async Task UpdateAsync(int id, LotDTO lot)
        {
            var response = await _http.PutAsJsonAsync($"api/lots/{id}", lot);

            if (!response.IsSuccessStatusCode)
                throw new Exception("No se pudo actualizar el lote.");
        }
        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/lots/{id}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("No se pudo eliminar el lote.");
        }
    }
}
