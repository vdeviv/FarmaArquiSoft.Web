
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.Services;
using FarmaArquiSoft.Web.DTOs;

public class Providers_IndexModel : PageModel {

    private readonly ProviderService _service;
    public IEnumerable<ProviderDTO> List {get;set;} = new List<ProviderDTO>();

    public Providers_IndexModel(ProviderService service){
        _service = service;
    }

    public async Task OnGet(){
        List = await _service.GetAllAsync();
    }

    public async Task<IActionResult> OnGetDelete(int id){
        await _service.DeleteAsync(id);
        return RedirectToPage("Index");
    }
}
