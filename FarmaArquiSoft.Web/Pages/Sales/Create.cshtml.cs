using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _http;

        public CreateModel(IHttpClientFactory http)
        {
            _http = http;
        }

        // ================= DATOS AUTOMÁTICOS =================
        public DateTime Fecha => DateTime.Now;
        public string Usuario => User.Identity?.Name ?? "usuario";

        // ================= CLIENTE =================
        [BindProperty]
        public string ClientId { get; set; } = string.Empty;

        [BindProperty]
        public string ClientName { get; set; } = string.Empty;

        // ================= DETALLE TEMPORAL =================
        [BindProperty]
        public List<SaleItemTempDTO> Items { get; set; } = new();

        public decimal Total => Items.Sum(i => i.SubTotal);

        // ================= LISTAS PARA MODALES =================
        public List<ClientDTO> Clientes { get; set; } = new();
        public List<LotDTO> Lotes { get; set; } = new();

        // ================= GET INICIAL =================
        public async Task OnGetAsync()
        {
            // ---- CLIENTES ----
            var clientApi = _http.CreateClient("ClientApi");
            Clientes = await clientApi
                .GetFromJsonAsync<List<ClientDTO>>("api/clients")
                ?? new();

            // ---- LOTES (PRECIO REAL) ----
            var lotApi = _http.CreateClient("LotApi");
            Lotes = await lotApi
                .GetFromJsonAsync<List<LotDTO>>("api/lots")
                ?? new();
        }

        // ================= SELECCIONAR CLIENTE =================
        public IActionResult OnPostSelectClient(string clientId, string clientName)
        {
            ClientId = clientId;
            ClientName = clientName;
            return Page();
        }

        // ================= AGREGAR ITEM DESDE LOTE =================
        public IActionResult OnPostAddItem(
            int lotId,
            int medId,
            string medName,
            string batchNumber,
            int quantity,
            decimal price)
        {
            var existing = Items.FirstOrDefault(i => i.LotId == lotId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                Items.Add(new SaleItemTempDTO
                {
                    LotId = lotId,
                    MedId = medId,
                    MedName = medName,
                    BatchNumber = batchNumber,
                    Quantity = quantity,
                    Price = price
                });
            }

            return Page();
        }

        // ================= QUITAR ITEM =================
        public IActionResult OnPostRemoveItem(int lotId)
        {
            Items.RemoveAll(i => i.LotId == lotId);
            return Page();
        }

        // ================= REGISTRAR VENTA =================
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(ClientId) || !Items.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Debe seleccionar un cliente y al menos un producto."
                );
                return Page();
            }

            var request = new CreateSaleRequestDTO
            {
                ClientId = ClientId,
                Items = Items.Select(i => new SaleItemDTO
                {
                    MedId = i.MedId.ToString(),
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            var saleApi = _http.CreateClient("SaleApi");

            // JWT
            var token = Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
            {
                saleApi.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            var response = await saleApi.PostAsync("api/sales", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Error al registrar la venta en el microservicio."
                );
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
