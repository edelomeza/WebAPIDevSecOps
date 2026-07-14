using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPIDevSecOps.Models
{
    [Table("VenVenta")]
    public class VenVenta
    {
        [Key]
        public int id { get; set; }

        public int idCliCliente { get; set; }

        public int idSegUsuario { get; set; }

        public int idVenCatEstado { get; set; }

        public DateTime? dteFechaHoraCompra { get; set; }

        [Required]
        [StringLength(10)]
        public string strClaveVenta { get; set; } = null!;

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        [ForeignKey("idCliCliente")]
        public CliCliente? CliCliente { get; set; }

        [ForeignKey("idSegUsuario")]
        public SegUsuario? SegUsuario { get; set; }

        [ForeignKey("idVenCatEstado")]
        public VenCatEstado? VenCatEstado { get; set; }
    }
}
