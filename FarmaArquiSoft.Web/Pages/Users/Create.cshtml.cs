using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceUser.Application.DTOS;
using ServiceUser.Application.Services;
using ServiceUser.Domain;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;


namespace FarmaView.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUserService _users;

        public CreateModel(IUserService users)
        {
            _users = users;
        }

        [BindProperty]
        public UserCreateVm Input { get; set; } = new();

        public SelectList Roles { get; private set; } = default!;

        public void OnGet()
        {
            LoadRoles();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            LoadRoles();
            if (!ModelState.IsValid) return Page();

            try
            {
                var dto = new UserCreateDto(
                    FirstName: Input.FirstName,
                    LastFirstName: Input.LastFirstName,
                    LastSecondName: Input.LastSecondName,
                    Mail: Input.Mail,
                    Phone: Input.Phone,
                    Ci: Input.Ci,
                    Role: Input.Role
                );

                const int actorId = 1;
                await _users.RegisterAsync(dto, actorId);

                TempData["SuccessMessage"] = "Usuario creado correctamente. Se envi? una contrase?a temporal al correo.";
                return RedirectToPage("Index");
            }
            catch (ServiceUser.Application.Services.ValidationException vex)
            {
                foreach (var kv in vex.Errors)
                    ModelState.AddModelError(kv.Key ?? string.Empty, kv.Value);
                return Page();
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        private void LoadRoles()
        {
            Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
        }

        public class UserCreateVm
        {
            [Required, Display(Name = "Nombre")]
            public string FirstName { get; set; } = "";

            [Required, Display(Name = "Primer Apellido")]
            public string LastFirstName { get; set; } = "";

            [Required, Display(Name = "Segundo Apellido")]
            public string LastSecondName { get; set; } = "";

            [Required, EmailAddress, Display(Name = "Correo")]
            public string Mail { get; set; } = "";

            [Required, Range(100000, 9999999999), Display(Name = "Teléfono")]
            public string Phone { get; set; }

            [Required, Display(Name = "CI")]
            public string Ci { get; set; } = "";

            [Required, Display(Name = "Rol")]
            public UserRole Role { get; set; } = UserRole.Cajero;
        }
    }
}
