using System.ComponentModel.DataAnnotations;

namespace WebAPIDevSecOps.Dto
{
    public class ProductoDeleteDto
    {
        public int id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
