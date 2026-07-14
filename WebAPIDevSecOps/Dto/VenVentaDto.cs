namespace WebAPIDevSecOps.Dto
{
    public class VenVentaDto
    {
        public int id { get; set; }
        public int idCliCliente { get; set; }
        public string? strNombreCliente { get; set; }
        public int idSegUsuario { get; set; }
        public string? strNombreUsuario { get; set; }
        public int idVenCatEstado { get; set; }
        public string? strEstado { get; set; }
        public DateTime? dteFechaHoraCompra { get; set; }
        public string strClaveVenta { get; set; } = null!;
        public byte[]? RowVersion { get; set; }
    }
}
