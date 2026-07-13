using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class CliClienteUpdateDto
    {
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ ]+$")]
        public string strNombreCliente { get; set; } = null!;

        [StringLength(200)]
        public string? strDireccionCliente { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string strCorreoElectronico { get; set; } = null!;

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$")]
        public string strNumeroTelefono { get; set; } = null!;

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
