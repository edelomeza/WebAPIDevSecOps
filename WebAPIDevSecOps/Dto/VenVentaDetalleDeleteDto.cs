using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class VenVentaDetalleDeleteDto
    {
        public int id { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
