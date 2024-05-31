using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La Descripción es Obligatoria")]
        public required string Descripcion { get; set; }

        [Required(ErrorMessage = "El Precio es Obligatorio")]
        public int Precio { get; set; }

        [Required(ErrorMessage = "La Cantidad es Obligatoria")]
        public int Cantidad { get; set; }

        public DateTime? Caducidad { get; set; }
    }
}
