
using MicroServicioProductos.DTOs;
using Microsoft.Data.SqlClient;
using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia;
using System.Configuration;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
{
    public class ProductoRepositorio : RepositorioBD, IRepositorio<Producto>
    {
        public int DescontarStock(int idProducto, int cantidad)
        {
            string query = @"UPDATE Producto 
                             SET Stock = Stock - @cantidad, 
                                 FechaUltimaActualizacion = @fechaAhora 
                             WHERE Id = @idProducto AND Stock >= @cantidad;";
            
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@idProducto", idProducto);
            command.Parameters.AddWithValue("@fechaAhora", DateTime.Now);

            return ExecuteNonQuery(command);
        }
        public List<ProductoDto> ObtenerDetallado()
        {
            string query = @"SELECT 
                                p.Id,
                                p.IdCategoria,
                                p.IdMarca,
                                p.Nombre,
                                p.Stock,
                                p.Estado,
                                p.FechaRegistro,
                                p.FechaUltimaActualizacion,
                                p.IdUsuario,
                                m.Nombre AS Marca,
                                c.Nombre AS Categoria
                            FROM Producto p
                            LEFT JOIN Marca m 
                                ON p.IdMarca = m.Id
                            LEFT JOIN Categoria c 
                                ON p.IdCategoria = c.Id
                            WHERE p.Estado = 1;";
            SqlCommand cmd = new SqlCommand(query);
            var result = new List<ProductoDto>();

            
            using (var reader = ExecuteReader(cmd))
            {
                while (reader.Read())
                {

                    result.Add(
                        new ProductoDto
                        {
                            Id = reader.GetInt32("Id"),
                            Nombre = reader["Nombre"].ToString(),
                            IdCategoria = reader.GetInt32("IdCategoria"),
                            IdMarca = reader.GetInt32("IdMarca"),
                            Stock = reader.GetInt32("Stock"),
                            NombreCategoria = reader["Categoria"].ToString(),
                            NombreMarca = reader["Marca"].ToString(),
                            FechaRegistro = (DateTime)reader["FechaRegistro"],
                            IdUsuario = reader.GetInt32("IdUsuario"),

                        });
                }
            }
            return result;
        }

        public Producto ObtenerPorId(int id)
        {
            string query = @"SELECT  Id, Nombre,IdCategoria,IdMarca,Stock,Estado,FechaRegistro,IdUsuario,FechaUltimaActualizacion
                            FROM Producto
                            WHERE Estado=1 and Id=@id
                            ORDER BY 3";
            
            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@id", id);
            Producto result= new Producto();
            using (var reader = ExecuteReader(command))
            {

                if (!reader.Read())
                    return null; //

                result =
                        new Producto
                        {
                            Id = reader.GetInt32("Id"),
                            Nombre = reader["Nombre"].ToString(),
                            IdCategoria = reader.GetInt32("IdCategoria"),
                            IdMarca = reader.GetInt32("IdMarca"),
                            Stock = reader.GetInt32("Stock"),
                            Estado = (bool)reader["Estado"],
                            FechaRegistro = (DateTime)reader["FechaRegistro"],
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            FechaUltimaActualizacion = (DateTime)reader["FechaUltimaActualizacion"]
                        };




                
            }
            return result;
        }

      

        

        public bool ExisteDuplicado(Producto producto)
        {
            return false;
        }
        public DataTable BuscarPorNombre(string frase)
        {
            frase = frase.ToLower();
            string query = @"
                            SELECT TOP 10 Nombre
                            FROM Producto
                            WHERE Estado = 1
                              AND Nombre LIKE @frase
                            ORDER BY Nombre ASC";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@frase", "%" + frase + "%");

            return ExecuteReturningDataTable(command);
        }
        public DataTable BuscarProducto(string nombre)
        {
            string query = @"
                            SELECT TOP 1
                                p.Id,
                                p.Nombre,
                                pp.Precio
                            FROM Producto p
                            INNER JOIN PresentacionProducto pp
                                ON p.Id = pp.IdProducto
                            WHERE p.Nombre LIKE @nombre
                              AND p.Estado = 1";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@nombre", "%" + nombre + "%");

            return ExecuteReturningDataTable(command);
        }

        public int RestaurarStock(int idProducto, int cantidad)
        {
            string query = @"UPDATE Producto 
                             SET Stock = Stock + @cantidad, 
                                 FechaUltimaActualizacion = @fechaAhora 
                             WHERE Id = @idProducto;";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@idProducto", idProducto);
            command.Parameters.AddWithValue("@fechaAhora", DateTime.Now);
            return ExecuteNonQuery(command);
        }

        public int Insertar(Producto t)
        {
            string query = @"INSERT INTO Producto ( Nombre,IdCategoria,IdMarca,Stock,IdUsuario)
                            VALUES (@nombre,@idCategoria,@idMarca,@stock,@idUsuario);
                            SELECT SCOPE_IDENTITY();";
            SqlCommand command = new SqlCommand(query);

            command.Parameters.AddWithValue("@nombre", t.Nombre);
            command.Parameters.AddWithValue("@idCategoria", t.IdCategoria);
            command.Parameters.AddWithValue("@idMarca", t.IdMarca);
            command.Parameters.AddWithValue("@stock", t.Stock);

            command.Parameters.AddWithValue("@idUsuario", t.IdUsuario);
            return Convert.ToInt32(RepositorioBD.Instancia.ExecuteScalar(command));
        }

        public int Actualizar(Producto t)
        {
            string query = @"UPDATE Producto
                            SET IdCategoria = @idCategoria,
	                            Nombre = @nombre,
                                IdMarca = @idMarca,
                                Stock = @stock,
                                FechaUltimaActualizacion = @fechaAhora,
                                IdUsuario=@idUsuario
                                
                            WHERE Id = @id;";

            SqlCommand command = new SqlCommand(query);

            command.Parameters.AddWithValue("@idCategoria", t.IdCategoria);
            command.Parameters.AddWithValue("@nombre", t.Nombre);
            command.Parameters.AddWithValue("@idMarca", t.IdMarca);
            command.Parameters.AddWithValue("@stock", t.Stock);
            command.Parameters.AddWithValue("@fechaAhora", DateTime.Now);
            command.Parameters.AddWithValue("@id", t.Id);
            command.Parameters.AddWithValue("@idUsuario", t.IdUsuario);
            return ExecuteNonQuery(command);
        }

        public int Eliminar(Producto t)        {
            string query = @"UPDATE Producto
                     SET Estado = 0, FechaUltimaActualizacion=@fechaAhora, IdUsuario=@idUsuario
                     WHERE Id = @Id";
            SqlCommand cmd = new SqlCommand(query);
            cmd.Parameters.AddWithValue("@fechaAhora", DateTime.Now);
            cmd.Parameters.AddWithValue("@idUsuario", t.IdUsuario);
            cmd.Parameters.AddWithValue("@Id", t.Id);

            return ExecuteNonQuery(cmd);
        }

        public List<Producto> ObtenerTodo()
        {
            string query = @"SELECT  Id, Nombre,IdCategoria,IdMarca,Stock,Estado,FechaRegistro,IdUsuario,FechaUltimaActualizacion
                            FROM Producto
                            WHERE Estado=1
                            ORDER BY 3";
            SqlCommand command = new SqlCommand(query);
            var result = new List<Producto>();
            using (var reader = ExecuteReader(command))
            {
                while (reader.Read())
                {

                    result.Add(
                        new Producto
                        {
                            Id = reader.GetInt32("Id"),
                            Nombre = reader["Nombre"].ToString(),
                            IdCategoria = reader.GetInt32("IdCategoria"),
                            IdMarca = reader.GetInt32("IdMarca"),
                            Stock = reader.GetInt32("Stock"),
                            Estado = (bool)reader["Estado"],
                            FechaRegistro = (DateTime)reader["FechaRegistro"],
                            IdUsuario = reader.GetInt32("IdUsuario"),
                            FechaUltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaActualizacion"))
                                ? (DateTime?)null
                                : reader.GetDateTime("FechaUltimaActualizacion")
                        }


                        );

                }
            }
            return result;

        }

        public DataRow ObtenerPorIdP(int id)
        {
            string query = @"SELECT  Id, Nombre,IdCategoria,IdMarca,Stock,Estado,FechaRegistro,IdUsuario,FechaUltimaActualizacion
                            FROM Producto
                            WHERE Estado=1 and Id=@id
                            ORDER BY 3";

            SqlCommand command = new SqlCommand(query);
            command.Parameters.AddWithValue("@id", id);

            return ExecuteReturningDataRow(command);
        }
    }
}
