using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class VenVentaUpdateDto
    {
        public int id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int idCliCliente { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int idSegUsuario { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int idVenCatEstado { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
