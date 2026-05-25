using MicroServicioProductos.Infraestructura.Persistencia;
using Microsoft.Data.SqlClient;

namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
{
    public class BitacoraRepositorio
    {
        public void Registrar(int idUsuario, string accion, string tabla, string descripcion)
        {
            string query = @"INSERT INTO Bitacora (IdUsuario, Accion, Tabla, Fecha, Descripcion) 
                             VALUES (@idUsuario, @accion, @tabla, GETDATE(), @descripcion)";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);
            command.Parameters.AddWithValue("@accion", accion);
            command.Parameters.AddWithValue("@tabla", tabla);
            command.Parameters.AddWithValue("@descripcion", descripcion);

            RepositorioBD.Instancia.ExecuteNonQuery(command);
        }
    }
}
