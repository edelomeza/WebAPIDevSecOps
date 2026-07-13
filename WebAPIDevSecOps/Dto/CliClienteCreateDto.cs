using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class CliClienteCreateDto
    {
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ ]+$")]
        public string strNombreCliente { get; set; }

        [StringLength(200)]
        public string? strDireccionCliente { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string strCorreoElectronico { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$")]
        public string strNumeroTelefono { get; set; }
    }
}
