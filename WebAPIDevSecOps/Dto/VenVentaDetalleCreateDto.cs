using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class VenVentaDetalleCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int idVenVenta { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int idProProducto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int intPiezaVenta { get; set; }
    }
}
