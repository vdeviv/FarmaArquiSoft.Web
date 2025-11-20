
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;

public class Providers_EditModel : PageModel {

    private readonly ProviderService _service;

    [BindProperty]
    public ProviderDTO Provider {get;set;} = new ProviderDTO();

    public Providers_EditModel(ProviderService service){
        _service = service;
    }

    public async Task OnGet(int id){
        Provider = await _service.GetAsync(id) ?? new ProviderDTO();
    }

    public async Task<IActionResult> OnPost(){
        if(!ModelState.IsValid) return Page();
        await _service.UpdateAsync(Provider);
        return RedirectToPage("Index");
    }
}
