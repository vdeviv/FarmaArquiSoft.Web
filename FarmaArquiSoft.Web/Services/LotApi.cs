using FarmaArquiSoft.Web.DTOs;
using System.Net.Http.Json;
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
            var list = await _http.GetFromJsonAsync<List<LotDTO>>("api/lots");
            return list ?? new List<LotDTO>();
        }

        public async Task<LotDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<LotDTO>($"api/lots/{id}");
        }

        // Crear un nuevo lote -> devolvemos HttpResponseMessage para usar ApiValidationFacade
        public Task<HttpResponseMessage> CreateAsync(LotDTO lot)
        {
            return _http.PostAsJsonAsync("api/lots", lot);
        }

        // Actualizar lote
        public Task<HttpResponseMessage> UpdateAsync(LotDTO lot)
        {
            return _http.PutAsJsonAsync($"api/lots/{lot.id}", lot);
        }

        // Eliminar lote
        public Task<HttpResponseMessage> DeleteAsync(int id)
        {
            return _http.DeleteAsync($"api/lots/{id}");
        }
    }
}
