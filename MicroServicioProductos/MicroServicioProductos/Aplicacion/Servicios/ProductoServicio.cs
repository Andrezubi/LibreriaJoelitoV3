

using MicroServicioProductos.Aplicacion.DTOs;
using MicroServicioProductos.Aplicacion.Results;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Dominio.Validadores;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Transactions;

namespace MicroServicioProductos.Aplicacion.Servicios
{
    public class ProductoServicio
    {

        private readonly ProductoRepositorio productoRepositorio;

        private readonly PresentacionProductoRepositorio presentacionProductoRepositorio;
        private readonly ProductoValidador productoValidador;
        

        public ProductoServicio(
            ProductoRepositorio productoRepository,
            PresentacionProductoRepositorio presentacionProductoRepository,
            ProductoValidador productoValidator)
        {
            this.productoRepositorio = productoRepository;
            this.presentacionProductoRepositorio= presentacionProductoRepository;
            this.productoValidador = productoValidator;
        }

        public List<Producto> ObtenerTodo()
        {
            return productoRepositorio.ObtenerTodo();
        }
        public List<ProductoDto> ObtenerProductosDetallados()
        {
            return productoRepositorio.ObtenerDetallado();
        }

        public Producto ObtenerPorId(int id)
        {
            return productoRepositorio.ObtenerPorId(id);
        }

        public Result<int> Insertar(Producto producto, int idPresentacion, int factorConversion, decimal precioVenta)
        {
            producto.Nombre = NormalizarTexto(producto.Nombre);
            var validationResults = productoValidador.ValidarProducto(producto);

            if (validationResults.Any())
            {
                var errores = validationResults
                    .Select(v => new ErrorValidacion(
                        v.MemberNames.FirstOrDefault() ?? string.Empty,
                        v.ErrorMessage ?? string.Empty))
                    .ToList();

                return Result<int>.Failure(errores);
            }

            // 1. Validaciones básicas
            if (idPresentacion <= 0) return Result<int>.Failure("Debe seleccionar una presentación válida.");
            if (factorConversion <= 0) return Result<int>.Failure("El factor de conversión debe ser mayor a cero.");
            if (precioVenta <= 0) return Result<int>.Failure("El precio de venta debe ser mayor a cero.");

            using (var scope = new TransactionScope())
            {
                try
                {
                    // 2. Insertamos el producto principal y recuperamos el ID generado
                    int nuevoIdProducto = productoRepositorio.Insertar(producto);
                    if (nuevoIdProducto <= 0) throw new Exception("Error al insertar el producto principal.");

                    // 3. VALIDACIÓN DE DUPLICADOS (Opcional aquí, pero recomendada)
                    Console.WriteLine($"idProducto: {nuevoIdProducto}            idPresentacion:{idPresentacion}");
                    var existente = presentacionProductoRepositorio.ObtenerPorIds(nuevoIdProducto, idPresentacion);
                    if (existente != null)
                        return Result<int>.Failure("Esta combinación de producto y presentación ya existe.");

                    // 4. Insertamos la relación
                    int relacionExitosa = presentacionProductoRepositorio.InsertarRelacion(
                        nuevoIdProducto, idPresentacion, factorConversion, precioVenta, producto.IdUsuario ?? 1);

                    if (relacionExitosa <= 0) throw new Exception("Error al asociar la presentación y el precio.");

                    scope.Complete();
                    return Result<int>.Success(nuevoIdProducto);
                }
                catch (Exception ex)
                {
                    return Result<int>.Failure($"Error en transacción: {ex.Message}");
                }
            }
        }

        // ---> EL NUEVO MÉTODO PARA AGREGAR PRESENTACIONES <---
        public Result AsociarNuevaPresentacion(int idProducto, int idPresentacion, int factor, decimal precio, int ?idUsuario)
        {
            try
            {
                // 1. Validaciones de negocio
                if (idProducto <= 0) return Result.Failure("Producto no válido.");
                if (idPresentacion <= 0) return Result.Failure("Debe seleccionar una presentación.");

                // 2. REVISIÓN DE DUPLICADOS: Consultamos si ya existe la llave compuesta
                var existente = presentacionProductoRepositorio.ObtenerPorIds(idProducto, idPresentacion);
                if (existente != null)
                {
                    return Result.Failure("Este producto ya tiene registrada esa presentación.");
                }

                // 3. Si no existe, procedemos con la inserción
                int filas = presentacionProductoRepositorio.InsertarRelacion(idProducto, idPresentacion, factor, precio, idUsuario);

                return filas > 0 ? Result.Success() : Result.Failure("No se pudo registrar la presentación.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Error de base de datos: " + ex.Message);
            }
        }

        public Result Actualizar(Producto producto)
        {
            producto.Nombre = NormalizarTexto(producto.Nombre);
            var validationResults = productoValidador.ValidarProducto(producto);

            if (validationResults.Any())
            {
                var errores = validationResults
                    .Select(v => new ErrorValidacion(
                        v.MemberNames.FirstOrDefault() ?? string.Empty,
                        v.ErrorMessage ?? string.Empty))
                    .ToList();

                return Result<int>.Failure(errores);
            }

            productoRepositorio.Actualizar(producto);
            return Result.Success();
        }

        public int Eliminar(Producto producto)
        {
            return productoRepositorio.Eliminar(producto);
        }

        public DataTable BuscarPorNombre(string frase)
        {
            return productoRepositorio.BuscarPorNombre(frase);
        }

        public DataTable BuscarProducto(string nombre)
        {
            return productoRepositorio.BuscarProducto(nombre);
        }


        public List<PresentacionProductoDto> ObtenerPresentacionesPorFrase(string frase)
        {
            return presentacionProductoRepositorio.obtenerPresentacionProductoDetallado(frase);
        }


        public PresentacionProductoDto? ObtenerPresentacionProducto(int idProducto, int idPresentacion)
        {
            var row = presentacionProductoRepositorio.ObtenerPorIds(
                idProducto,
                idPresentacion);

            if (row == null)
                return null;

            return new PresentacionProductoDto
            {
                IdProducto = Convert.ToInt32(row["IdProducto"]),
                IdPresentacion = Convert.ToInt32(row["IdPresentacion"]),
                Precio = Convert.ToDecimal(row["Precio"]),
                FactorConversion = Convert.ToInt32(row["FactorConversion"]),
                Producto = row["Producto"].ToString(),
                Presentacion = row["Presentacion"].ToString(),
                Marca = row["Marca"].ToString(),
                Descripcion = row["Descripcion"].ToString()
            };
        }

        public string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = Regex.Replace(texto.Trim(), @"\s+", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        }


    }
}