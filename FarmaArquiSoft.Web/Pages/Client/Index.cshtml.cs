using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FarmaArquiSoft.Web.Pages.Client
{
    public class Index : PageModel
    {
        private readonly ClientApi _clientApi;

        public List<ClientDTO> Clientes { get; set; } = new();

        public Index(ClientApi clientApi)
        {
            _clientApi = clientApi;
        }

        public async Task OnGet()
        {
            try
            {
                Clientes = await _clientApi.GetAllAsync();
                Clientes = Clientes.OrderBy(c => c.first_name).ToList();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al cargar clientes: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                var res = await _clientApi.DeleteAsync(id);

                if (res.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Cliente eliminado correctamente.";
                }
                else if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"Cliente con ID {id} no existe.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        $"Error al eliminar. Código {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión con el API: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
