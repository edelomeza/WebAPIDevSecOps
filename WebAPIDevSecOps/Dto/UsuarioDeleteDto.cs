using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class UsuarioDeleteDto
    {
        public int id { get; set; }

        ////comentar
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
