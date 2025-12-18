using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly SaleApi _saleApi;
        private readonly MedicineApi _medicineApi;
        private readonly ClientApi _clientApi;
        private readonly LotApi _lotApi; // <--- NUEVO: Para sacar el precio

        public CreateModel(
            SaleApi saleApi,
            MedicineApi medicineApi,
            ClientApi clientApi,
            LotApi lotApi)
        {
            _saleApi = saleApi;
            _medicineApi = medicineApi;
            _clientApi = clientApi;
            _lotApi = lotApi;
        }

        // ================= DATOS DE CABECERA =================
        public DateTime Fecha => DateTime.Now;
        public string Usuario => User.Identity?.Name ?? "Admin";

        // ================= CLIENTE =================
        [BindProperty]
        public string ClientId { get; set; } = string.Empty;

        [BindProperty]
        public string ClientName { get; set; } = string.Empty;

        // ================= PRODUCTOS =================
        public List<SelectListItem> MedicineOptions { get; set; } = new();

        [BindProperty]
        public List<SaleItemPayload> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.Price * i.Quantity);

        // ================= HANDLERS =================

        public async Task OnGetAsync()
        {
            await LoadMedicines();
        }

        // 🔍 HANDLER 1: BUSCAR CLIENTE
        public async Task<IActionResult> OnGetSearchClientAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new JsonResult(new { success = false });

            try
            {
                var clients = await _clientApi.SearchAsync(query);
                var client = clients.FirstOrDefault(c => c.nit == query);

                if (client != null)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        name = $"{client.first_name} {client.last_name}"
                    });
                }
                return new JsonResult(new { success = false });
            }
            catch
            {
                return new JsonResult(new { success = false });
            }
        }

        // 💰 HANDLER 2: OBTENER PRECIO DEL LOTE (NUEVO)
        public async Task<IActionResult> OnGetGetPriceAsync(int medId)
        {
            try
            {
                // Buscamos los lotes de ese medicamento
                var lots = await _lotApi.GetByMedicineAsync(medId);

                // Filtramos: Que tenga stock (> 0) y no esté eliminado
                // (Asumimos que el backend ya los ordena por vencimiento)
                var activeLot = lots.FirstOrDefault(l => l.quantity > 0 && !l.is_deleted);

                if (activeLot != null)
                {
                    return new JsonResult(new
                    {
                        success = true,
                        price = activeLot.unit_cost,
                        stock = activeLot.quantity
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Sin stock disponible"
                    });
                }
            }
            catch
            {
                return new JsonResult(new { success = false, message = "Error al consultar precio" });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                ModelState.AddModelError(string.Empty, "Debe ingresar un CI/NIT de cliente.");
                await LoadMedicines();
                return Page();
            }

            if (Items == null || !Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");
                await LoadMedicines();
                return Page();
            }

            var saleRequest = new CreateSaleRequest
            {
                ClientId = ClientId,
                Items = Items
            };

            try
            {
                var response = await _saleApi.CreateSaleAsync(saleRequest);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Venta registrada exitosamente.";
                    return RedirectToPage("/Lots/Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error al registrar: {response.ReasonPhrase}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión: {ex.Message}");
            }

            await LoadMedicines();
            return Page();
        }

        private async Task LoadMedicines()
        {
            try
            {
                var meds = await _medicineApi.GetAllAsync();
                MedicineOptions = meds.Where(m => !m.IsDeleted).Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Name} ({m.Presentation})"
                }).OrderBy(t => t.Text).ToList();
            }
            catch
            {
                MedicineOptions = new List<SelectListItem>();
            }
        }
    }
}