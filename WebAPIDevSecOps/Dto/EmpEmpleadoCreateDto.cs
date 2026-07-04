using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class EmpEmpleadoCreateDto
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_ ]+$")]
        public string strNombre { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúñÑ ]+$")]
        public string? strAPaterno { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[a-zA-ZáéíóúñÑ ]+$")]
        public string? strAMaterno { get; set; }

        [StringLength(18)]
        [RegularExpression(@"^[A-Z0-9]{18}$")]
        public string? strCURP { get; set; }

        public int? idEmpCatTipoEmpleado { get; set; }
    }
}
