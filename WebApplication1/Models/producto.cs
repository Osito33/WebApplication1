using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El NDC es Obligatorio")]
        [StringLength(11, ErrorMessage = "El NDC no puede tener más de 10 caracteres.")]
        [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "El NDC solo puede contener números y guiones.")]
        public string NDC { get; set; }

        [Required(ErrorMessage = "La Descripción es Obligatoria")]
        [StringLength(25, ErrorMessage = "La descripción no puede tener más de 25 caracteres.")]
        public required string Descripcion { get; set; }

        [Required(ErrorMessage = "El Precio es Obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser un número entero positivo")]
        public int Precio { get; set; }

        [Required(ErrorMessage = "El contenido es Obligatorio")]
        [StringLength(15, ErrorMessage = "El contenido no puede tener más de 15 caracteres.")]
        public required string Contenido { get; set; }

        [Required(ErrorMessage = "La Caducidad es Obligatoria")]
        public DateOnly Caducidad { get; set; }
    }
}
