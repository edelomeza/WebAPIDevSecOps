using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class EmpEmpleadoDto
    {
        public int id { get; set; }
        public string strNombre { get; set; }
        public string? strAPaterno { get; set; }
        public string? strAMaterno { get; set; }
        public string? strCURP { get; set; }
        public int? idEmpCatTipoEmpleado { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
