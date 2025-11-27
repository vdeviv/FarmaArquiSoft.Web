using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FarmaArquiSoft.Web.Pages.Providers
{
    public class Index : PageModel
    {
        private readonly ProviderApi _providerApi;

        public Index(ProviderApi providerApi)
        {
            _providerApi = providerApi;
        }

        public List<ProviderDTO> Providers { get; private set; } = new();

        public async Task OnGetAsync()
        {
            TempData.Clear();
            try
            {
                Providers = await _providerApi.GetAllAsync();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] =
                    $"Error de conexión con el API de Proveedores: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"Ocurrió un error inesperado al cargar proveedores: {ex.Message}";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var res = await _providerApi.DeleteAsync(id);

                if (res.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Proveedor eliminado correctamente.";
                }
                else if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"El proveedor con ID {id} no existe.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        $"No se pudo eliminar. Código: {(int)res.StatusCode}, Detalle: {res.ReasonPhrase}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] =
                    $"Error de conexión con el API al eliminar proveedor: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"Error inesperado al eliminar proveedor: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
