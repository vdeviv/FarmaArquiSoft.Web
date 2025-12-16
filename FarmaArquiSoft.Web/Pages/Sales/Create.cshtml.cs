using FarmaArquiSoft.Web.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FarmaArquiSoft.Web.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

        public void OnGet()
        {
            // UI solamente
        }

        // ================= POST REAL AL SALE API =================
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(ClientId) || !Items.Any())
            {
                ModelState.AddModelError(string.Empty,
                    "Debe seleccionar un cliente y al menos un producto.");
                return Page();
            }

            var request = new CreateSaleRequestDto
            {
                ClientId = ClientId,
                Items = Items.Select(i => new SaleItemDto
                {
                    MedId = i.MedId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            var client = _httpClientFactory.CreateClient("SaleApi");

            // JWT
            var token = Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/sales", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    "Error al registrar la venta en el microservicio.");
                return Page();
            }

            return RedirectToPage("/Index");
        }

        // ================= QUITAR ITEM =================
        public IActionResult OnPostRemoveItem(string medId)
        {
            Items.RemoveAll(i => i.MedId == medId);
            return Page();
        }
    }
}
