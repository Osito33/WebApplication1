using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public string Descripcion { get; set; }
        public string Contenido { get; set; }
        public int Precio { get; set; }
        public int Cantidad { get; set; }
    }
}
