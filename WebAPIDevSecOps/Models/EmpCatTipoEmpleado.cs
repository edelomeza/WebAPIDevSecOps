using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Models
{
    public class EmpCatTipoEmpleado
    {
        [Key]
        public int id { get; set; }

        [StringLength(50)]
        public string strValor { get; set; }

        [StringLength(150)]
        public string strDescripcion { get; set; }

    }
}
