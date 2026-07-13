using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class CliClienteDto
    {
        public int id { get; set; }
        public string strNombreCliente { get; set; }
        public string? strDireccionCliente { get; set; }
        public string strCorreoElectronico { get; set; }
        public string strNumeroTelefono { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
