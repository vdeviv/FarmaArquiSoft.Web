using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceCommon.Domain.Ports;
using ServiceUser.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaView.Pages.Users
{
    [Authorize(Roles = "Administrador")]
    public class IndexModel : PageModel
    {
        private readonly IUserService _users;
        private readonly IEncryptionService _encryptionService;

        public IEnumerable<User> Users { get; private set; } = new List<User>();
        public Dictionary<int, string> EncryptedIds { get; private set; } = new Dictionary<int, string>();

        public IndexModel(IUserService users, IEncryptionService encryptionService)
        {
            _users = users;
            _encryptionService = encryptionService;
        }

        public async Task OnGetAsync()
        {
            Users = await _users.ListAsync();
            foreach (var user in Users)
            {
                var encryptedId = _encryptionService.EncryptId(user.id);
                EncryptedIds.Add(user.id, encryptedId);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            const int ACTOR_ID = 1;

            try
            {
                await _users.SoftDeleteAsync(id, ACTOR_ID);
                TempData["SuccessMessage"] = $"Usuario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el usuario: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}