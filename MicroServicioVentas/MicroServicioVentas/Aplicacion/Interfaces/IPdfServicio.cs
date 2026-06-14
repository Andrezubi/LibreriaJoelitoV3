namespace MicroServicioVentas.Aplicacion.Interfaces
{
    public interface IPdfServicio
    {
        byte[] GenerarComprobanteVenta(System.Data.DataTable datosVenta);
    }
}