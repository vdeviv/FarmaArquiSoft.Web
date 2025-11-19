using System.ComponentModel.DataAnnotations;

namespace FarmaArquiSoft.Web.DTOs
{
    public class ClientDTO
    {
        public int id { get; set; }

        [Display(Name = "Nombre")]
        public string first_name { get; set; } = string.Empty;

        [Display(Name = "Apellido")]
        public string last_name { get; set; } = string.Empty;

        [Display(Name = "NIT / C.I.")]
        public string nit { get; set; } = string.Empty;

        [Display(Name = "Correo Electrónico")]
        public string? email { get; set; }
    }
}
