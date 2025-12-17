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
        private readonly MedicineApi _medicineApi; // 1. Inyectamos

        public List<LotDTO> Lots { get; private set; } = new();

        public IndexModel(LotApi lotApi, MedicineApi medicineApi)
        {
            _lotApi = lotApi;
            _medicineApi = medicineApi;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // 2. Carga paralela
                var lotsTask = _lotApi.GetAllAsync();
                var medicinesTask = _medicineApi.GetAllAsync();

                await Task.WhenAll(lotsTask, medicinesTask);

                Lots = await lotsTask;
                var medicines = await medicinesTask;

                // 3. Diccionario: ID Medicina -> Nombre + Presentación
                var medDict = medicines.ToDictionary(m => m.Id, m => $"{m.Name} ({m.Presentation})");

                // 4. Mapeo
                foreach (var lot in Lots)
                {
                    if (medDict.TryGetValue(lot.medicine_id, out var name))
                    {
                        lot.MedicineName = name;
                    }
                    else
                    {
                        lot.MedicineName = $"Medicina ID: {lot.medicine_id}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cargando datos: {ex.Message}";
            }
        }

        // ... (El Delete sigue igual) ...
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
