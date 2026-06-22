using MicroServicioReportes.Aplicacion.Interfaces;
using MicroServicioReportes.Dominio.Entidades;
using MicroServicioReportes.Infraestructura.Persistencia;
using MySql.Data.MySqlClient;
using System.Data;

namespace MicroServicioReportes.Infraestructura.Repositorios
{
    public class ComprobanteVentaRepositorio : IComprobanteVentaRepositorio
    {
        private readonly RepositorioBD _bd;

        public ComprobanteVentaRepositorio(RepositorioBD bd)
        {
            _bd = bd;
        }

        public bool ExistePorVentaId(int ventaId)
        {
            var comando = new MySqlCommand(@"
                SELECT COUNT(1)
                FROM comprobantes_venta
                WHERE venta_id = @ventaId;
            ");

            comando.Parameters.AddWithValue("@ventaId", ventaId);

            object? resultado = _bd.ExecuteScalar(comando);

            return Convert.ToInt32(resultado) > 0;
        }

        public int RegistrarComprobante(ComprobanteVenta comprobante)
        {
            var comando = new MySqlCommand(@"
                INSERT INTO comprobantes_venta (
                    venta_id,
                    correlation_id,
                    message_id,
                    numero_comprobante,
                    cliente_id,
                    cliente_nombre,
                    cliente_ci_nit,
                    usuario_id,
                    usuario_nombre,
                    fecha_venta,
                    fecha_generacion,
                    total,
                    estado,
                    fecha_anulacion,
                    creado_en,
                    actualizado_en
                )
                VALUES (
                    @ventaId,
                    @correlationId,
                    @messageId,
                    @numeroComprobante,
                    @clienteId,
                    @clienteNombre,
                    @clienteCiNit,
                    @usuarioId,
                    @usuarioNombre,
                    @fechaVenta,
                    @fechaGeneracion,
                    @total,
                    @estado,
                    @fechaAnulacion,
                    @creadoEn,
                    @actualizadoEn
                );
            ");

            comando.Parameters.AddWithValue("@ventaId", comprobante.VentaId);
            comando.Parameters.AddWithValue("@correlationId", comprobante.CorrelationId);
            comando.Parameters.AddWithValue("@messageId", comprobante.MessageId);
            comando.Parameters.AddWithValue("@numeroComprobante", comprobante.NumeroComprobante);

            comando.Parameters.AddWithValue("@clienteId", (object?)comprobante.ClienteId ?? DBNull.Value);
            comando.Parameters.AddWithValue("@clienteNombre", comprobante.ClienteNombre);
            comando.Parameters.AddWithValue("@clienteCiNit", (object?)comprobante.ClienteCiNit ?? DBNull.Value);

            comando.Parameters.AddWithValue("@usuarioId", (object?)comprobante.UsuarioId ?? DBNull.Value);
            comando.Parameters.AddWithValue("@usuarioNombre", comprobante.UsuarioNombre);

            comando.Parameters.AddWithValue("@fechaVenta", comprobante.FechaVenta);
            comando.Parameters.AddWithValue("@fechaGeneracion", comprobante.FechaGeneracion);
            comando.Parameters.AddWithValue("@total", comprobante.Total);

            comando.Parameters.AddWithValue("@estado", comprobante.Estado);
            comando.Parameters.AddWithValue("@fechaAnulacion", (object?)comprobante.FechaAnulacion ?? DBNull.Value);

            comando.Parameters.AddWithValue("@creadoEn", comprobante.CreadoEn);
            comando.Parameters.AddWithValue("@actualizadoEn", (object?)comprobante.ActualizadoEn ?? DBNull.Value);

            _bd.ExecuteNonQuery(comando);

            return Convert.ToInt32(comando.LastInsertedId);
        }

        public void RegistrarDetalles(
            int comprobanteVentaId,
            IEnumerable<ComprobanteVentaDetalle> detalles
        )
        {
            foreach (var detalle in detalles)
            {
                var comando = new MySqlCommand(@"
                    INSERT INTO comprobante_venta_detalles (
                        comprobante_venta_id,
                        producto_id,
                        producto_nombre,
                        cantidad,
                        precio_unitario,
                        subtotal
                    )
                    VALUES (
                        @comprobanteVentaId,
                        @productoId,
                        @productoNombre,
                        @cantidad,
                        @precioUnitario,
                        @subtotal
                    );
                ");

                comando.Parameters.AddWithValue("@comprobanteVentaId", comprobanteVentaId);
                comando.Parameters.AddWithValue("@productoId", detalle.ProductoId);
                comando.Parameters.AddWithValue("@productoNombre", detalle.ProductoNombre);
                comando.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                comando.Parameters.AddWithValue("@precioUnitario", detalle.PrecioUnitario);
                comando.Parameters.AddWithValue("@subtotal", detalle.Subtotal);

                _bd.ExecuteNonQuery(comando);
            }
        }

        public ComprobanteVenta? ObtenerPorVentaId(int ventaId)
        {
            var comandoCabecera = new MySqlCommand(@"
                SELECT
                    id,
                    venta_id,
                    correlation_id,
                    message_id,
                    numero_comprobante,
                    cliente_id,
                    cliente_nombre,
                    cliente_ci_nit,
                    usuario_id,
                    usuario_nombre,
                    fecha_venta,
                    fecha_generacion,
                    total,
                    estado,
                    fecha_anulacion,
                    creado_en,
                    actualizado_en
                FROM comprobantes_venta
                WHERE venta_id = @ventaId
                LIMIT 1;
            ");

            comandoCabecera.Parameters.AddWithValue("@ventaId", ventaId);

            DataRow? fila = _bd.ExecuteReturningDataRow(comandoCabecera);

            if (fila == null)
                return null;

            var comprobante = MapearComprobante(fila);

            var comandoDetalles = new MySqlCommand(@"
                SELECT
                    id,
                    comprobante_venta_id,
                    producto_id,
                    producto_nombre,
                    cantidad,
                    precio_unitario,
                    subtotal
                FROM comprobante_venta_detalles
                WHERE comprobante_venta_id = @comprobanteVentaId;
            ");

            comandoDetalles.Parameters.AddWithValue("@comprobanteVentaId", comprobante.Id);

            DataTable tablaDetalles = _bd.ExecuteReturningDataTable(comandoDetalles);

            foreach (DataRow filaDetalle in tablaDetalles.Rows)
            {
                comprobante.Detalles.Add(MapearDetalle(filaDetalle));
            }

            return comprobante;
        }

        public void MarcarComoAnulado(int ventaId, DateTime fechaAnulacion)
        {
            var comando = new MySqlCommand(@"
                UPDATE comprobantes_venta
                SET
                    estado = 'ANULADO',
                    fecha_anulacion = @fechaAnulacion,
                    actualizado_en = @actualizadoEn
                WHERE venta_id = @ventaId;
            ");

            comando.Parameters.AddWithValue("@ventaId", ventaId);
            comando.Parameters.AddWithValue("@fechaAnulacion", fechaAnulacion);
            comando.Parameters.AddWithValue("@actualizadoEn", DateTime.Now);

            _bd.ExecuteNonQuery(comando);
        }

        private static ComprobanteVenta MapearComprobante(DataRow fila)
        {
            return new ComprobanteVenta
            {
                Id = Convert.ToInt32(fila["id"]),
                VentaId = Convert.ToInt32(fila["venta_id"]),
                CorrelationId = Convert.ToString(fila["correlation_id"]) ?? string.Empty,
                MessageId = Convert.ToString(fila["message_id"]) ?? string.Empty,
                NumeroComprobante = Convert.ToString(fila["numero_comprobante"]) ?? string.Empty,

                ClienteId = fila["cliente_id"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(fila["cliente_id"]),

                ClienteNombre = Convert.ToString(fila["cliente_nombre"]) ?? string.Empty,

                ClienteCiNit = fila["cliente_ci_nit"] == DBNull.Value
                    ? null
                    : Convert.ToString(fila["cliente_ci_nit"]),

                UsuarioId = fila["usuario_id"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(fila["usuario_id"]),

                UsuarioNombre = Convert.ToString(fila["usuario_nombre"]) ?? string.Empty,

                FechaVenta = Convert.ToDateTime(fila["fecha_venta"]),
                FechaGeneracion = Convert.ToDateTime(fila["fecha_generacion"]),

                Total = Convert.ToDecimal(fila["total"]),
                Estado = Convert.ToString(fila["estado"]) ?? string.Empty,

                FechaAnulacion = fila["fecha_anulacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(fila["fecha_anulacion"]),

                CreadoEn = Convert.ToDateTime(fila["creado_en"]),

                ActualizadoEn = fila["actualizado_en"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(fila["actualizado_en"])
            };
        }

        private static ComprobanteVentaDetalle MapearDetalle(DataRow fila)
        {
            return new ComprobanteVentaDetalle
            {
                Id = Convert.ToInt32(fila["id"]),
                ComprobanteVentaId = Convert.ToInt32(fila["comprobante_venta_id"]),
                ProductoId = Convert.ToInt32(fila["producto_id"]),
                ProductoNombre = Convert.ToString(fila["producto_nombre"]) ?? string.Empty,
                Cantidad = Convert.ToInt32(fila["cantidad"]),
                PrecioUnitario = Convert.ToDecimal(fila["precio_unitario"]),
                Subtotal = Convert.ToDecimal(fila["subtotal"])
            };
        }
    }
}