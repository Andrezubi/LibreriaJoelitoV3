using MySql.Data.MySqlClient;
using MicroServicioVentas.Aplicacion.Interfaces;
using MicroServicioVentas.Aplicacion.Results;
using MicroServicioVentas.Dominio.Modelos;
using MicroServicioVentas.Infraestructura.Persistencia;
using MicroServicioVentas.Infraestructura.Persistencia.FactoriaProductos;
using System.Data;

namespace MicroServicioVentas.Aplicacion.Servicios
{
    public class RealizarVentaServicio 
    {
        private readonly VentaRepositorio _ventaRepositorio;
        private readonly DetalleVentaRepositorio _detalleVentaRepositorio;
        private readonly ProductoRepositorio _productoRepositorio;
        private readonly ClienteRepositorio _clienteRepositorio;
        private readonly PresentacionProductoRepositorio _presentaProdRepositorio;

        public RealizarVentaServicio(
            PresentacionProductoRepositorio presentProdRepositorio,
            VentaRepositorio ventaRepositorio,
            DetalleVentaRepositorio detalleVentaRepositorio,
            ProductoRepositorio productoRepositorio,
            ClienteRepositorio clienteRepositorio
        )
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _productoRepositorio = productoRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _presentaProdRepositorio = presentProdRepositorio;
        }

        public Result<int> RegistrarVenta(Venta venta, List<DetalleVenta> detalles)
        {
            try
            {
                // 1. Validaciones previas (Fuera de transacción para no bloquear)
                if (detalles == null || !detalles.Any())
                    return Result<int>.Failure("La venta debe tener al menos un producto.");

                var clienteFila = _clienteRepositorio.ObtenerPorIdDR(venta.IdCliente);
                if (clienteFila == null)
                    return Result<int>.Failure("El cliente seleccionado no es válido.");

                // 2. Iniciar Proceso Atómico
                RepositorioBD.Instancia.BeginTransaction();

                try
                {
                    // 3. Insertar Cabecera de Venta
                    int ventaId = _ventaRepositorio.Insertar(venta);
                    if (ventaId <= 0)
                        throw new Exception("No se pudo generar la cabecera de la venta.");

                    // 4. Procesar Detalles y Stock
                    foreach (var detalle in detalles)
                    {
                        detalle.IdVenta = ventaId;

                        // Insertar Detalle
                        int filasDetalle = _detalleVentaRepositorio.Insertar(detalle);
                        if (filasDetalle <= 0)
                            throw new Exception($"Error al insertar el detalle para el producto: {_productoRepositorio.ObtenerPorIdP(detalle.IdProducto)?["Nombre"]}");

                        // --- NUEVA LÓGICA DE FACTOR DE CONVERSIÓN ---

                        // A) Consultamos la presentación a la base de datos para obtener el factor de forma segura
                        DataRow presentacionFila = _presentaProdRepositorio.ObtenerPorIds(detalle.IdProducto, detalle.IdPresentacion);
                        if (presentacionFila == null)
                            throw new Exception("No se encontró la presentación del producto especificado.");

                        int factorConversion = Convert.ToInt32(presentacionFila["FactorConversion"]);

                        // B) Calculamos la cantidad real a descontar del inventario general (unidades)
                        int cantidadRealADescontar = detalle.Cantidad * factorConversion;

                        // C) Descontamos el stock usando la cantidad real multiplicada
                        int filasStock = _productoRepositorio.DescontarStock(detalle.IdProducto, cantidadRealADescontar);
                        if (filasStock <= 0)
                        {
                            // Si no afectó filas es porque el Stock < CantidadReal (validación lógica en el SQL)
                            throw new Exception($"Stock insuficiente para el producto: {_productoRepositorio.ObtenerPorIdP(detalle.IdProducto)?["Nombre"]}");
                        }
                    }

                    // 5. Confirmar todo
                    RepositorioBD.Instancia.Commit();
                    return Result<int>.Success(ventaId);
                }
                catch (Exception ex)
                {
                    // 6. Revertir si algo falló
                    RepositorioBD.Instancia.Rollback();
                    return Result<int>.Failure($"Error en la transacción: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Error inesperado: {ex.Message}");
            }
        }
    }
}