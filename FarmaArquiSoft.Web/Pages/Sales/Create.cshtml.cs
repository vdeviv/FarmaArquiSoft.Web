using FarmaArquiSoft.Web.DTOs;
using FarmaArquiSoft.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly SaleApi _saleApi;
        private readonly MedicineApi _medicineApi;
        private readonly ClientApi _clientApi;
        private readonly LotApi _lotApi;

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

        // ================= CABECERA =================
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

        // ================= GET =================
        public async Task OnGetAsync()
        {
            await LoadMedicines();
        }

        // ================= BUSCAR CLIENTE =================
        public async Task<IActionResult> OnGetSearchClientAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new JsonResult(new { success = false });

            try
            {
                var clients = await _clientApi.SearchAsync(query);
                var client = clients.FirstOrDefault(c => c.nit == query);

                if (client == null)
                    return new JsonResult(new { success = false });

                return new JsonResult(new
                {
                    success = true,
                    id = client.id,
                    name = $"{client.first_name} {client.last_name}"
                });
            }
            catch
            {
                return new JsonResult(new { success = false });
            }
        }

        // ================= CREAR CLIENTE (MODAL) =================
        public async Task<IActionResult> OnPostCreateClientAsync(
            string firstName,
            string lastName,
            string nit,
            string email)
        {
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(nit))
            {
                return new JsonResult(new { success = false });
            }

            var dto = new ClientDTO
            {
                first_name = firstName,
                last_name = lastName,
                nit = nit,
                email = email
            };

            try
            {
                var response = await _clientApi.CreateAsync(dto);

                if (!response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false });
                }

                // 🔹 LEEMOS EL JSON DEVUELTO POR EL MICROSERVICIO
                var json = await response.Content.ReadAsStringAsync();
                var created = JsonSerializer.Deserialize<ClientDTO>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (created == null)
                {
                    return new JsonResult(new { success = false });
                }

                return new JsonResult(new
                {
                    success = true,
                    id = created.id,
                    name = $"{created.first_name} {created.last_name}"
                });
            }
            catch
            {
                return new JsonResult(new { success = false });
            }
        }

        // ================= PRECIO DESDE LOTES =================
        public async Task<IActionResult> OnGetGetPriceAsync(int medId)
        {
            try
            {
                var lots = await _lotApi.GetByMedicineAsync(medId);
                var lot = lots.FirstOrDefault(l => l.quantity > 0 && !l.is_deleted);

                if (lot == null)
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "Sin stock"
                    });
                }

                return new JsonResult(new
                {
                    success = true,
                    price = lot.unit_cost,
                    stock = lot.quantity
                });
            }
            catch
            {
                return new JsonResult(new { success = false });
            }
        }

        // ================= REGISTRAR VENTA =================
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un cliente.");
                await LoadMedicines();
                return Page();
            }

            if (!Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");
                await LoadMedicines();
                return Page();
            }

            var request = new CreateSaleRequest
            {
                ClientId = ClientId,
                Items = Items
            };

            try
            {
                var response = await _saleApi.CreateSaleAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Venta registrada correctamente.";
                    return RedirectToPage("/Lots/Index");
                }

                ModelState.AddModelError(string.Empty, "Error al registrar la venta.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await LoadMedicines();
            return Page();
        }

        // ================= HELPERS =================
        private async Task LoadMedicines()
        {
            try
            {
                var meds = await _medicineApi.GetAllAsync();

                MedicineOptions = meds
                    .Where(m => !m.IsDeleted)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = $"{m.Name} ({m.Presentation})"
                    })
                    .OrderBy(m => m.Text)
                    .ToList();
            }
            catch
            {
                MedicineOptions = new List<SelectListItem>();
            }
        }
    }
}
