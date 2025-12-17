using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FarmaArquiSoft.Web.Pages.Medicines
{
    public class Index : PageModel
    {
        private readonly MedicineApi _medicineApi;
        private readonly ProviderApi _providerApi; // 1. Inyectamos ProviderApi

        public Index(MedicineApi medicineApi, ProviderApi providerApi)
        {
            _medicineApi = medicineApi;
            _providerApi = providerApi;
        }

        public List<MedicineDTO> Medicines { get; private set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // 2. Traemos ambas listas en paralelo (m?s r?pido)
                var medicinesTask = _medicineApi.GetAllAsync();
                var providersTask = _providerApi.GetAllAsync();

                await Task.WhenAll(medicinesTask, providersTask);

                Medicines = await medicinesTask;
                var providers = await providersTask;

                // 3. Creamos un Diccionario para b?squeda r?pida (O(1))
                // Clave: ID Proveedor -> Valor: Nombre Completo
                var providerDict = providers.ToDictionary(p => p.id, p => $"{p.first_name} {p.last_name}");

                // 4. "JOIN" en memoria
                foreach (var m in Medicines)
                {
                    if (providerDict.TryGetValue(m.ProviderId, out var name))
                    {
                        m.ProviderName = name;
                    }
                    else
                    {
                        m.ProviderName = "Desconocido/Eliminado";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cargando datos: {ex.Message}";
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var res = await _medicineApi.DeleteAsync(id);
                if (res.IsSuccessStatusCode) TempData["SuccessMessage"] = "Eliminado correctamente.";
                else TempData["ErrorMessage"] = "No se pudo eliminar.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
