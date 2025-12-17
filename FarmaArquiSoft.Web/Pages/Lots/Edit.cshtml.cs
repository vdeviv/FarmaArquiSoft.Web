using System.Net;
using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Lots
{
    public class EditModel : PageModel
    {
        private readonly LotApi _lotApi;
        private readonly MedicineApi _medicineApi;

        [BindProperty]
        public LotDTO Input { get; set; } = new();

        public List<SelectListItem> MedicineOptions { get; set; } = new();

        public EditModel(LotApi lotApi, MedicineApi medicineApi)
        {
            _lotApi = lotApi;
            _medicineApi = medicineApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var lot = await _lotApi.GetByIdAsync(id);
                if (lot == null)
                {
                    TempData["ErrorMessage"] = "Lote no encontrado.";
                    return RedirectToPage("Index");
                }
                Input = lot;

                await LoadMedicines();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToPage("Index");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadMedicines();
                return Page();
            }

            try
            {
                var response = await _lotApi.UpdateAsync(Input);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Lote actualizado.";
                    return RedirectToPage("Index");
                }

                // ... (Mismo manejo de errores BadRequest que en Create) ...
                // Si quieres el código completo del BadRequest dímelo, pero es igual al Create

                ModelState.AddModelError(string.Empty, $"Error API: {response.StatusCode}");
                await LoadMedicines();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                await LoadMedicines();
                return Page();
            }
        }

        private async Task LoadMedicines()
        {
            var medicines = await _medicineApi.GetAllAsync();
            MedicineOptions = medicines.Where(m => !m.IsDeleted).Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Name} ({m.Presentation})"
            }).OrderBy(t => t.Text).ToList();
        }
    }
}