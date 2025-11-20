
using System.Net.Http.Json;
using FarmaArquiSoft.Web.DTOs;

namespace FarmaArquiSoft.Web.Services;

public class ProviderService {
    private readonly HttpClient _http;
    private readonly string _base = "http://localhost:5009/api/providers";

    public ProviderService(HttpClient http){ _http=http; }

    public async Task<IEnumerable<ProviderDTO>> GetAllAsync(){
        return await _http.GetFromJsonAsync<IEnumerable<ProviderDTO>>(_base) 
               ?? new List<ProviderDTO>();
    }

    public async Task<ProviderDTO?> GetAsync(int id){
        return await _http.GetFromJsonAsync<ProviderDTO>($"{_base}/{id}");
    }

    public async Task<bool> CreateAsync(ProviderDTO dto){
        var res = await _http.PostAsJsonAsync(_base,dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(ProviderDTO dto){
        var res = await _http.PutAsJsonAsync($"{_base}/{dto.Id}",dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id){
        var res = await _http.DeleteAsync($"{_base}/{id}");
        return res.IsSuccessStatusCode;
    }
}
