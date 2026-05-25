using Microsoft.Data.SqlClient;
using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia;
using System.Data;

namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
{
    public class MarcaRepositorio : RepositorioBD, IRepositorio<Marca>
    {
        public int Eliminar(Marca t)
        {
            SqlCommand cmd = new SqlCommand(@"
                UPDATE Marca SET
                    Estado                   = 0,
                    IdUsuario                = @idUsuario,
                    FechaUltimaActualizacion = GETDATE()
                WHERE Id = @id");

            cmd.Parameters.AddWithValue("@idUsuario", t.IdUsuario);
            cmd.Parameters.AddWithValue("@id", t.Id);
            return ExecuteNonQuery(cmd);
        }

        public List<Marca> ObtenerTodo()
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT Id, Nombre, Descripcion, PaginaWeb, Industria, FechaRegistro, IdUsuario
                FROM Marca
                WHERE Estado = 1
                ORDER BY Nombre");

            var result = new List<Marca>();
            var reader = ExecuteReader(cmd);
            while (reader.Read())
            {
                result.Add(new Marca
                {
                    Id = reader.GetInt32("Id"),
                    Nombre = reader["Nombre"].ToString()!,
                    Descripcion = reader["Descripcion"] == DBNull.Value ? null : reader["Descripcion"].ToString(),
                    PaginaWeb = reader["PaginaWeb"] == DBNull.Value ? null : reader["PaginaWeb"].ToString(),
                    Industria = reader["Industria"] == DBNull.Value ? null : reader["Industria"].ToString(),
                    FechaRegistro = (DateTime)reader["FechaRegistro"],
                    IdUsuario = reader.GetInt32("IdUsuario")
                });
            }
            return result;
        }

        public Marca ObtenerPorId(int id)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT Id, Nombre, Descripcion, PaginaWeb, Industria, FechaRegistro, IdUsuario
                FROM Marca
                WHERE Id = @id AND Estado = 1");

            cmd.Parameters.AddWithValue("@id", id);

            var result = new Marca();
            var reader = ExecuteReader(cmd);
            while (reader.Read())
            {
                result = new Marca
                {
                    Id = reader.GetInt32("Id"),
                    Nombre = reader["Nombre"].ToString()!,
                    Descripcion = reader["Descripcion"] == DBNull.Value ? null : reader["Descripcion"].ToString(),
                    PaginaWeb = reader["PaginaWeb"] == DBNull.Value ? null : reader["PaginaWeb"].ToString(),
                    Industria = reader["Industria"] == DBNull.Value ? null : reader["Industria"].ToString(),
                    FechaRegistro = (DateTime)reader["FechaRegistro"],
                    IdUsuario = reader.GetInt32("IdUsuario")
                };
            }
            return result;
        }

        public int Insertar(Marca t)
        {
            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Marca (Nombre, Descripcion, PaginaWeb, Industria, IdUsuario)
                VALUES (@nombre, @descripcion, @paginaWeb, @industria, @idUsuario);
                SELECT SCOPE_IDENTITY();");

            AgregarParametros(cmd, t);
            return Convert.ToInt32(ExecuteScalar(cmd));
        }

        public int Actualizar(Marca t)
        {
            SqlCommand cmd = new SqlCommand(@"
                UPDATE Marca SET
                    Nombre                   = @nombre,
                    Descripcion              = @descripcion,
                    PaginaWeb                = @paginaWeb,
                    Industria                = @industria,
                    IdUsuario                = @idUsuario,
                    FechaUltimaActualizacion = GETDATE()
                WHERE Id = @id");

            AgregarParametros(cmd, t);
            cmd.Parameters.AddWithValue("@id", t.Id);
            return ExecuteNonQuery(cmd);
        }

        public bool ExisteDuplicado(Marca marca)
        {
            SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Marca
                WHERE UPPER(TRIM(Nombre)) = UPPER(TRIM(@nombre))
                  AND Id    <> @id
                  AND Estado = 1");

            cmd.Parameters.AddWithValue("@nombre", marca.Nombre);
            cmd.Parameters.AddWithValue("@id", marca.Id);
            return Convert.ToInt32(ExecuteScalar(cmd)) > 0;
        }

        // --- Métodos privados de apoyo ---

        private static void AgregarParametros(SqlCommand cmd, Marca marca)
        {
            cmd.Parameters.AddWithValue("@nombre", marca.Nombre);
            cmd.Parameters.AddWithValue("@descripcion", (object?)marca.Descripcion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@paginaWeb", (object?)marca.PaginaWeb ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@industria", (object?)marca.Industria ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@idUsuario", marca.IdUsuario);
        }
    }
}