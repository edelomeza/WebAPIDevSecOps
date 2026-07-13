using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class ProductoUpdateDto
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ ]+$")]
        public string strNombreProducto { get; set; } = null!;

        [StringLength(300)]
        [Url]
        public string? strURLImagen { get; set; }

        [StringLength(250)]
        public string? strDescripcion { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int intNumeroExistencia { get; set; }

        [Required]
        [Range(0.01, 9999999.99)]
        public decimal decPrecio { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
