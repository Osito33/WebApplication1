using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "La Descripción es Obligatoria")]
        public string NombreUsuario { get; set; }
        public string Contraseña { get; set; }

        public string Rol { get; set; }
        // Otros campos relevantes
    }

}
