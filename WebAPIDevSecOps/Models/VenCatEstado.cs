using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Models
{
    public class VenCatEstado
    {
        [Key]
        public int id { get; set; }

        [StringLength(50)]
        public string strValor { get; set; } = null!;

        [StringLength(200)]
        public string? strDescripcion { get; set; }
    }
}
