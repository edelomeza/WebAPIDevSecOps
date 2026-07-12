using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPIDevSecOps.Models
{
    [Table("SegTokenBlacklist")]
    public class SegTokenBlacklist
    {
        [Key]
        [StringLength(500)]
        public string Jti { get; set; } = string.Empty;

        public DateTime ExpiryUtc { get; set; }
    }
}
