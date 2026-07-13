using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPIDevSecOps.Models
{
    [Table("EmpEmpleado")]
    public class EmpEmpleado
    {
        [Key]
        public int id { get; set; }

        [StringLength(50)]
        public string strNombre { get; set; } = null!;

        [StringLength(50)]
        public string? strAPaterno { get; set; }

        [StringLength(50)]
        public string? strAMaterno { get; set; }

        [StringLength(18)]
        public string? strCURP { get; set; }

        public int? idEmpCatTipoEmpleado { get; set; }

        [ForeignKey("idEmpCatTipoEmpleado")]
        public EmpCatTipoEmpleado? EmpCatTipoEmpleado { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
