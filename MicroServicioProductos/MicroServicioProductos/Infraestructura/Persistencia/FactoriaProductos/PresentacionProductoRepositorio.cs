
using Microsoft.Data.SqlClient;
using MicroServicioProductos.Aplicacion.DTOs;
using MicroServicioProductos.Aplicacion.Interfaces;
using MicroServicioProductos.Dominio.Modelos;
using System.Data;
using System.Reflection.PortableExecutable;

namespace MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos
{
    public class PresentacionProductoRepositorio : RepositorioBD, IRepositorio<PresentacionProducto>
    {
        public int Actualizar(PresentacionProducto t)
        {
            throw new NotImplementedException();
        }

        public int Eliminar(PresentacionProducto t)
        {
            throw new NotImplementedException();
        }

        public int Insertar(PresentacionProducto t)
        {
            throw new NotImplementedException();
        }

        public List<PresentacionProducto> ObtenerTodo()
        {
            throw new NotImplementedException();
        }


        public DataRow? ObtenerPorIds(int idProducto, int idPresentacion)
        {
            string query = @"
                                SELECT 
                                    pp.IdProducto,
                                    pp.IdPresentacion,
                                    pp.Precio,
                                    pp.FactorConversion AS FactorConversion,
                                    p.Nombre AS Producto,
                                    pr.Nombre AS Presentacion,
                                    m.Nombre AS Marca,
                                    CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) AS Descripcion
                                FROM PresentacionProducto pp
                                INNER JOIN Producto p ON pp.IdProducto = p.Id
                                INNER JOIN Presentacion pr ON pp.IdPresentacion = pr.Id
                                LEFT JOIN Marca m ON p.IdMarca = m.Id
                                WHERE pp.IdProducto = @idProducto
                                  AND pp.IdPresentacion = @idPresentacion
                                  AND pp.Estado = 1
                                  AND p.Estado = 1
                                  AND pr.Estado = 1";

            var cmd = new SqlCommand(query);

            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idPresentacion", idPresentacion);

            var dt = ExecuteReturningDataTable(cmd);

            if (dt.Rows.Count > 0)
                return dt.Rows[0];

            return null;
        }

        public PresentacionProducto? ObtenerEntidadPorIds(int idProducto, int idPresentacion)
        {
            string query = @"SELECT
                        IdProducto,
                        IdPresentacion,
                        FactorConversion,
                        Precio,
                        Estado,
                        FechaRegistro,
                        FechaUltimaActualizacion,
                        IdUsuario
                    FROM PresentacionProducto
                    WHERE IdProducto=@idProducto
                    AND IdPresentacion=@idPresentacion
                    AND Estado=1";

            SqlCommand cmd = new SqlCommand(query);

            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idPresentacion", idPresentacion);

            using var reader = ExecuteReader(cmd);

            if (!reader.Read())
                return null;

            return new PresentacionProducto
            {
                IdProducto = reader.GetInt32("IdProducto"),
                IdPresentacion = reader.GetInt32("IdPresentacion"),
                FactorConversion = reader.GetInt32("FactorConversion"),
                Precio = reader.GetDecimal("Precio"),
                Estado = reader.GetBoolean("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaUltimaActualizacion =
                    reader.IsDBNull(reader.GetOrdinal("FechaUltimaActualizacion"))
                        ? null
                        : reader.GetDateTime("FechaUltimaActualizacion"),

                IdUsuario =
                    reader.IsDBNull(reader.GetOrdinal("IdUsuario"))
                        ? null
                        : reader.GetInt32("IdUsuario")
            };
        }


        public List<PresentacionProductoDto> obtenerPresentacionProductoDetallado(string frase)
        {
            string query = @"SELECT 
                                pp.IdProducto,
                                pp.IdPresentacion,
                                pp.Estado AS EstadoPresentacionProducto,
                                pp.FactorConversion as FactorConversion,
                                p.Nombre AS Producto,
                                pr.Nombre AS Presentacion,
                                m.Nombre AS Marca,
                                CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) AS Descripcion,
                                pp.Precio
                            FROM PresentacionProducto pp
                            INNER JOIN Producto p 
                                ON pp.IdProducto = p.Id
                            INNER JOIN Presentacion pr 
                                ON pp.IdPresentacion = pr.Id
                            LEFT JOIN Marca m 
                                ON p.IdMarca = m.Id
                            WHERE CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) 
                                  LIKE CONCAT('%', @frase, '%')
                              AND pp.Estado = 1
                              AND p.Estado = 1
                              AND pr.Estado = 1
                              AND (m.Estado = 1 OR m.Id IS NULL);";
            SqlCommand cmd = new SqlCommand(query);
            cmd.Parameters.AddWithValue("@frase", frase);

            List<PresentacionProductoDto> result= new List<PresentacionProductoDto>();

            using (var reader = ExecuteReader(cmd))
            {

                while (reader.Read())
                {

                    result.Add(new PresentacionProductoDto
                    {
                        IdProducto = reader.GetInt32("IdProducto"),
                        IdPresentacion = reader.GetInt32("IdPresentacion"),
                        Precio = reader.GetDecimal("Precio"),
                        FactorConversion = reader.GetInt32("FactorConversion"),
                        Producto = reader["Producto"].ToString(),
                        Presentacion = reader["Presentacion"].ToString(),
                        Marca = reader["Marca"].ToString(),
                        Descripcion = reader["Descripcion"].ToString()
                    });
                }
            }
            return result;
        }


        public int InsertarRelacion(int idProducto, int idPresentacion, double factorConversion, decimal precio, int? idUsuario)
        {
            string query = @"INSERT INTO PresentacionProducto (IdProducto, IdPresentacion, FactorConversion, Precio, IdUsuario) 
                     VALUES (@idProd, @idPres, @factor, @precio, @idUsu)";

            SqlCommand cmd = new SqlCommand(query);
            cmd.Parameters.AddWithValue("@idProd", idProducto);
            cmd.Parameters.AddWithValue("@idPres", idPresentacion);
            cmd.Parameters.AddWithValue("@factor", factorConversion);
            cmd.Parameters.AddWithValue("@precio", precio);
            cmd.Parameters.AddWithValue("@idUsu", idUsuario);

            return ExecuteNonQuery(cmd);
        }

    }

        

        
    
}
