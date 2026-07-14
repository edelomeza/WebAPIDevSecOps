namespace WebAPIDevSecOps.Dto
{
    public class VenVentaDetalleDto
    {
        public int id { get; set; }
        public int idVenVenta { get; set; }
        public int idProProducto { get; set; }
        public string? strNombreProducto { get; set; }
        public decimal decPrecio { get; set; }
        public int intPiezaVenta { get; set; }
        public decimal decTotalVenta { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}
