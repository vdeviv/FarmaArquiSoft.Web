using System.ComponentModel.DataAnnotations;

namespace FarmaArquiSoft.Web.DTOs
{
        public class ClientDTO
        {
            public int id { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [Display(Name = "Nombre")]
            public string first_name { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es obligatorio.")]
            [Display(Name = "Apellido")]
            public string last_name { get; set; } = string.Empty;

            [Required(ErrorMessage = "El NIT/C.I. es obligatorio.")]
            [Display(Name = "NIT / C.I.")]
            public string nit { get; set; } = string.Empty;

            [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
            [Display(Name = "Correo Electrónico")]
            public string? email { get; set; }
        }
    }
