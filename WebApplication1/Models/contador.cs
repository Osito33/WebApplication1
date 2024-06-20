using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class Contador
    {
        public int Id { get; set; }
        public int contadorVentas { get; set; }

    }

}
