using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El # de venta es Obligatorio")]
        public int NoVenta { get; set; }

        [Required(ErrorMessage = "El NDC es Obligatorio")]
        public required string NDC { get; set; }

        [Required(ErrorMessage = "La Descripción es Obligatoria")]
        public required string Descripcion { get; set; }
         
        [Required(ErrorMessage = "La Cantidad es Obligatoria")]
        public required string Cantidad { get; set; }

        [Required(ErrorMessage = "La Caducidad es Obligatoria")]
        public DateOnly Caducidad { get; set; }

        [Required(ErrorMessage = "El Precio es Obligatorio")]
        public int Precio { get; set; }

        public int Total { get; set; }
    }
}
