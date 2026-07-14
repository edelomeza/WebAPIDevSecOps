using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class VenVentaCreateDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int idCliCliente { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int idSegUsuario { get; set; }
    }
}
