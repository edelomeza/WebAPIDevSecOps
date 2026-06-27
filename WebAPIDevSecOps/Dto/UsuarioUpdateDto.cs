using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class UsuarioUpdateDto
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_ ]+$")]
        public string strNombre { get; set; }

        //[Required]
        //[MinLength(8)]
        public string? strPWD { get; set; }

        [Required]
        [StringLength(50)]
        [EmailAddress]
        public string strCorreoElectronico { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
