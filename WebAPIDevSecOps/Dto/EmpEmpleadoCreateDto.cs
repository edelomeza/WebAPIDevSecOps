using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class EmpEmpleadoCreateDto
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_ ]+$")]
        public string strNombre { get; set; } = null!;

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúñÑ ]+$")]
        public string? strAPaterno { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúñÑ ]+$")]
        public string? strAMaterno { get; set; }

        [StringLength(18)]
        [RegularExpression(@"^[A-Z]{1}[AEIOUX]{1}[A-Z]{2}[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])[HM]{1}(AS|BC|BS|CC|CH|CL|CM|CS|DF|DG|GR|HG|JC|MC|MN|MS|NT|NL|OC|PL|QT|QR|SP|SL|SR|TC|TS|TL|VZ|YN|ZS|NE)[B-DF-HJ-NP-TV-Z]{3}[A-Z0-9]{1}[0-9]{1}$")]
        public string? strCURP { get; set; }

        public int? idEmpCatTipoEmpleado { get; set; }
    }
}
