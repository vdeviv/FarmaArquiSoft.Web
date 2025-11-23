using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class IndexModel : PageModel
    {
        private readonly LotApi _lotApi;

        public List<LotDTO> Lots { get; private set; } = new();

        public IndexModel(LotApi lotApi)
        {
            _lotApi = lotApi;
        }

        public async Task OnGetAsync()
        {
            try
            {
                Lots = await _lotApi.GetAllAsync();
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión al cargar lotes: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocurrió un error inesperado al cargar lotes: {ex.Message}";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID de lote inválido.";
                return RedirectToPage();
            }

            try
            {
                var res = await _lotApi.DeleteAsync(id);

                if (res.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Lote eliminado correctamente.";
                }
                else if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = $"El lote con ID {id} no existe.";
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
                    $"Error de conexión con el API al eliminar: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    $"Error inesperado al eliminar: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
