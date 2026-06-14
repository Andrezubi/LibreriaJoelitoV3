using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.DTOs;
using MicroServicioVentas.Aplicacion.DTOs.ServicioVentaDTOs;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Dominio.Modelos;
using System.Data;

namespace MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos
{
    public class VentaRepositorio : RepositorioBD, IRepositorio<Venta>
    {
        public int Insertar(Venta venta)
        {
            string consulta = @"INSERT INTO venta (IdCliente,Total,IdUsuario)
                                VALUES (@idCliente,@total,@idUsuario);
                                SELECT LAST_INSERT_ID();";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            comando.Parameters.AddWithValue("@total", venta.Total);
            comando.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);

            return Convert.ToInt32(ExecuteScalar(comando));
        }

        public int Eliminar(Venta venta)
        {
            string consulta = @"UPDATE venta
                                SET Estado = 0, FechaUltimaActualizacion=@fechaAhora, IdUsuario=@idUsuario
                                WHERE Id = @Id";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@fechaAhora", DateTime.Now);
            comando.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
            comando.Parameters.AddWithValue("@Id", venta.Id);

            return ExecuteNonQuery(comando);
        }

        public List<Venta> ObtenerTodo()
        {
            string consulta = @"SELECT  Id, IdCliente, Fecha, Total, FechaRegistro, IdUsuario
                                FROM venta
                                WHERE Estado=1
                                ORDER BY 3";
            MySqlCommand comando = new MySqlCommand(consulta);

            var result = new List<Venta>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                result.Add(new Venta
                {
                    Id = reader.GetInt32("Id"),
                    IdCliente = reader.GetInt32("IdCliente"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = reader.GetDecimal("Total"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                    IdUsuario = reader.GetInt32("IdUsuario")
                });
            }

            return result;
        }

        public Venta? ObtenerPorId(int id)
        {
            string consulta = @"SELECT Id, IdCliente, Fecha, Total, FechaRegistro, FechaUltimaActualizacion, IdUsuario
                        FROM venta
                        WHERE Estado = 1 AND Id = @id
                        ORDER BY 3";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@id", id);

            var reader = ExecuteReader(comando);

            if (!reader.Read())
                return null;

            return new Venta
            {
                Id = reader.GetInt32("Id"),
                IdCliente = reader.GetInt32("IdCliente"),
                Fecha = reader.GetDateTime("Fecha"),
                Total = reader.GetDecimal("Total"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaUltimaActualizacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaActualizacion"))
                    ? null
                    : reader.GetDateTime("FechaUltimaActualizacion"),
                IdUsuario = reader.GetInt32("IdUsuario")
            };
        }

        public VentaCabeceraDTO? ObtenerCabeceraVentaPorId(int id)
        {
            string consulta = @"SELECT v.Id,
                               v.Estado AS EstadoVenta,
                               c.Ci AS CiCliente,
                               c.RazonSocial AS RS,
                               u.Nombre AS NombreEmpleado,
                               v.Fecha,
                               v.Total
                        FROM venta v
                        INNER JOIN cliente c ON v.IdCliente = c.Id
                        INNER JOIN usuario u ON v.IdUsuario = u.Id
                        WHERE v.Estado = 1 AND v.Id = @id
                        ORDER BY v.Fecha DESC";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@id", id);

            var reader = ExecuteReader(comando);

            if (!reader.Read())
                return null;

            return new VentaCabeceraDTO
            {
                Id = reader.GetInt32("Id"),
                EstadoVenta = reader.GetInt32("EstadoVenta"),
                CiCliente = reader.GetInt32("CiCliente"),
                NombreCliente = reader.GetString("RS"),
                NombreEmpleado = reader.GetString("NombreEmpleado"),
                Fecha = reader.GetDateTime("Fecha"),
                Total = reader.GetDecimal("Total")
            };
        }

        public List<Venta> ObtenerPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            string consulta = @"SELECT Id, IdCliente, Fecha, Total, FechaRegistro, IdUsuario, estado
                        FROM venta
                        WHERE Estado = 1
                            AND Fecha BETWEEN @fechaInicio AND @fechaFin
                        ORDER BY 3";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@fechaInicio", fechaInicio);
            comando.Parameters.AddWithValue("@fechaFin", fechaFin);

            var ventas = new List<Venta>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                ventas.Add(new Venta
                {
                    Id = reader.GetInt32("Id"),
                    IdCliente = reader.GetInt32("IdCliente"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = reader.GetDecimal("Total"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                    FechaUltimaActualizacion = null,
                    IdUsuario = reader.GetInt32("IdUsuario"),
                });
            }

            return ventas;
        }

        public List<Venta> ObtenerPorIdCliente(int idCliente)
        {
            string consulta = @"SELECT Id, IdCliente, Fecha, Total, FechaRegistro, IdUsuario
                        FROM venta
                        WHERE Estado = 1 
                            AND IdCliente = @idCliente
                        ORDER BY 3";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@idCliente", idCliente);

            var ventas = new List<Venta>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                ventas.Add(new Venta
                {
                    Id = reader.GetInt32("Id"),
                    IdCliente = reader.GetInt32("IdCliente"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = reader.GetDecimal("Total"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                    FechaUltimaActualizacion = null,
                    IdUsuario = reader.GetInt32("IdUsuario")
                });
            }

            return ventas;
        }

        public int Actualizar(Venta venta)
        {
            string consulta = @"UPDATE venta
                                SET IdCliente = @idCliente,
                                    Fecha = @fecha,
                                    Total = @total,
                                    FechaUltimaActualizacion=@fechaAhora,
                                    IdUsuario=@idUsuario
                                WHERE Id = @Id";
            MySqlCommand comando = new MySqlCommand(consulta);

            comando.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            comando.Parameters.AddWithValue("@fecha", venta.Fecha);
            comando.Parameters.AddWithValue("@total", venta.Total);
            comando.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
            comando.Parameters.AddWithValue("@fechaAhora", DateTime.Now);
            comando.Parameters.AddWithValue("@Id", venta.Id);

            return ExecuteNonQuery(comando);
        }

        public DataTable ObtenerDatosComprobante(int idVenta)
        {
            string consulta = @"SELECT 
                                    v.Id AS VentaId, 
                                    v.Fecha, 
                                    v.Total,
                                    v.FechaRegistro,
                                    c.Ci, 
                                    c.Complemento, 
                                    c.RazonSocial as RS,
                                    u.Username AS NombreEmpleado,
                                    dv.Cantidad, 
                                    CONCAT(pr.Nombre, ' de ', p.Nombre, ' ', m.Nombre) AS DescripcionProducto,
                                    dv.PrecioUnitario, 
                                    dv.Subtotal
                                FROM venta v
                                INNER JOIN cliente c ON v.IdCliente = c.Id
                                INNER JOIN usuario u ON v.IdUsuario = u.Id
                                INNER JOIN detalleventa dv ON v.Id = dv.IdVenta
                                INNER JOIN producto p ON dv.IdProducto = p.Id
                                INNER JOIN marca m ON p.IdMarca = m.Id
                                INNER JOIN presentacion pr ON dv.IdPresentacion = pr.Id
                                WHERE v.Id = @idVenta AND v.Estado = 1";

            MySqlCommand comando = new MySqlCommand(consulta);
            comando.Parameters.AddWithValue("@idVenta", idVenta);

            return ExecuteReturningDataTable(comando);
        }

        public List<VentaDTO> CargarVentas()
        {
            string consulta = @"SELECT v.Id,
                                    v.Estado AS EstadoVenta,
                                    c.Ci AS CiCliente,
                                    c.RazonSocial AS NombreCliente,
                                    v.Fecha
                                FROM venta v
                                INNER JOIN cliente c ON v.IdCliente = c.Id
                                INNER JOIN usuario u ON v.IdUsuario = u.Id
                                ORDER BY v.Fecha DESC";

            MySqlCommand comando = new MySqlCommand(consulta);

            var resultado = new List<VentaDTO>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                resultado.Add(new VentaDTO
                {
                Id = reader.GetInt32("Id"),
                Estado = reader.GetInt32("EstadoVenta"),
                CiCliente = reader.GetInt32("CiCliente"),
                NombreCliente = reader.GetString("NombreCliente"),
                Fecha = reader.GetDateTime("Fecha")
             });
             }

            return resultado;
        }
        public List<Reporte1DTO> ObtenerReporteServicios()
        {
            string consulta = @"
                        SET @n := 0;
                        SELECT 
                            @n := @n + 1 AS Nro,
                            p.Nombre AS 'Nombre del Servicio',
                            AVG(dv.PrecioUnitario) AS 'Costo Bs.',
                            '' AS Descripción, 
                            c.Nombre AS Categoría,
                            SUM(dv.Cantidad) AS 'Cantidad Total Vendida'
                        FROM 
                            DetalleVenta dv
                        JOIN 
                            Producto p ON dv.IdProducto = p.Id
                        JOIN 
                            Categoria c ON p.IdCategoria = c.Id
                        JOIN 
                            Venta v ON dv.IdVenta = v.Id
                        GROUP BY 
                            p.Id, c.Id
                        ORDER BY 
                            p.Nombre;";

            MySqlCommand comando = new MySqlCommand(consulta);

            var resultado = new List<Reporte1DTO>();
            var reader = ExecuteReader(comando);

            while (reader.Read())
            {
                resultado.Add(new Reporte1DTO
                {
                    Nro = Convert.ToInt32(reader["Nro"]),
                    NombreServicio = reader["Nombre del Servicio"].ToString(),
                    CostoBs = Convert.ToDecimal(reader["Costo Bs."]),
                    Descripcion = reader["Descripción"].ToString(),
                    Categoria = reader["Categoría"].ToString()
                });
            }
            reader.Close();

            return resultado;
        }
    }
}
