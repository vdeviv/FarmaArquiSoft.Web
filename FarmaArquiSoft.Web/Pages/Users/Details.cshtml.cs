using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceUser.Domain;
using System.Security.Claims;

namespace FarmaView.Pages.Users
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUserService _users;
        public DetailsModel(IUserService users) => _users = users;

        public User? Current { get; private set; }

        public async Task OnGet()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idStr, out var userId))
                Current = await _users.GetByIdAsync(userId);
        }
    }
}
