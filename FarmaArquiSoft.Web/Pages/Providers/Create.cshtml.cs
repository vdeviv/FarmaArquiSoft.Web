
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;

public class Providers_CreateModel : PageModel {

    private readonly ProviderService _service;

    [BindProperty]
    public ProviderDTO Provider {get;set;} = new ProviderDTO();

    public Providers_CreateModel(ProviderService service){
        _service = service;
    }

    public async Task<IActionResult> OnPost(){
        if(!ModelState.IsValid) return Page();
        await _service.CreateAsync(Provider);
        return RedirectToPage("Index");
    }
}
