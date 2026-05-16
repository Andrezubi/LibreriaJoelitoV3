using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using MicroServicioProductos.Aplicacion.DTOs;
using MicroServicioProductos.Aplicacion.Results;
using MicroServicioProductos.Aplicacion.Servicios;

using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Dominio.Validadores;
using MicroServicioProductos.Infraestructura.Persistencia;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroServicioProductos.Controllers
{
    // Controllers/ProductosController.cs
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductoController : ControllerBase
    {
        private readonly ProductoServicio _productoServicio;
        private readonly PresentacionServicio _presentacionServicio;
        private readonly Infrestructura.Persistencia.FactoriaProductos.BitacoraRepositorio _bitacoraRepo;
        
        public ProductoController(
            ProductoServicio productoServicio, 
            PresentacionServicio presentacionServicio,
            Infrestructura.Persistencia.FactoriaProductos.BitacoraRepositorio bitacoraRepo)
        {
            _productoServicio = productoServicio;
            _presentacionServicio = presentacionServicio;
            _bitacoraRepo = bitacoraRepo;
        }

        private int GetIdUsuarioFromHeader()
        {
            if (Request.Headers.TryGetValue("X-IdUsuario", out var idStr))
            {
                if (int.TryParse(idStr, out int id)) return id;
            }
            return 0;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_productoServicio.ObtenerProductosDetallados());
        [HttpGet("categorias")]
        public IActionResult GetCategorias() {

            string query = @"SELECT Id, Nombre FROM categoria WHERE estado = 1 ORDER BY Nombre";
            MySqlCommand cmd = new MySqlCommand(query);

            List<CategoriaDto> result = new List<CategoriaDto>();
            MySqlDataReader reader = RepositorioBD.Instancia.ExecuteReader(cmd);
            while (reader.Read())
            {

                result.Add(
                    new CategoriaDto
                    {
                        Id = reader.GetInt32("Id"),
                        Nombre = reader["Nombre"].ToString(),
                        
                    });
            }
            return Ok(result);


        }

        [HttpGet("marcas")]
        public IActionResult GetMarcas() {
            string query = @"SELECT Id, Nombre FROM marca WHERE estado = 1 ORDER BY Nombre";
            MySqlCommand cmd = new MySqlCommand(query);

            List<Marca> result = new List<Marca>();
            MySqlDataReader reader = RepositorioBD.Instancia.ExecuteReader(cmd);
            while (reader.Read())
            {

                result.Add(
                    new Marca
                    {
                        Id = reader.GetInt32("Id"),
                        Nombre = reader["Nombre"].ToString(),

                    });
            }
            return Ok(result);
        }

        [HttpPost("categorias")]
        public IActionResult InsertCategorias([FromBody] CategoriaDto data) {
            data.Nombre = data.Nombre?.Trim();
            if (string.IsNullOrWhiteSpace(data.Nombre)) return BadRequest(new { errores = "No se puede estar en blanco la categoria" });

            try
            {
                var errors = ExtraValidador.ValidarNombreCategoria(data.Nombre);
                if (errors.Any()) return BadRequest(new { errores = errors });

                string query = "INSERT INTO categoria (Nombre, IdUsuario) VALUES (@nombre, @idUsuario);";
                MySqlCommand cmd = new MySqlCommand(query);
                cmd.Parameters.AddWithValue("@nombre", data.Nombre);
                cmd.Parameters.AddWithValue("@idUsuario",data.IdUsuario);
                int res=RepositorioBD.Instancia.ExecuteNonQuery(cmd);
                if (res >= 1) {
                    // AUDITORÍA
                    _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "INSERT", "Categoria", $"Nueva categoría creada: {data.Nombre}");
                    return Ok(new { success = true });
                }
                return BadRequest(new { errores = "No se ingeso correctamente" });
            }
            catch (Exception ex)
            {
               return BadRequest(ex);
            }
        } 

        [HttpGet("presentaciones")]
        public IActionResult GetPresentaciones() => Ok(_presentacionServicio.ObtenerTodo());



        [HttpPost("{idPresentacion}/{factorConversion}/{precioVenta}")]
        public IActionResult Create(int idPresentacion,int factorConversion,decimal precioVenta ,[FromBody] Producto producto) 
        {
            var result = _productoServicio.Insertar(producto,idPresentacion,factorConversion,precioVenta);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });
            
            // AUDITORÍA
            _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "INSERT", "Producto", $"Nuevo producto registrado con ID: {result.Value}");
            return Ok(new {success=true});
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Producto producto)
        {
            var result = _productoServicio.Actualizar(producto);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });
            
            // AUDITORÍA
            _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "UPDATE", "Producto", $"Producto actualizado ID: {id}");
            return Ok(new { success = true });
        }



        [HttpDelete("{id}")]
        public IActionResult Delete(int id, [FromQuery] int idUsuario)
        {
            Console.WriteLine($"Id:{id}   idUsuario:{idUsuario}");
            var producto = new Producto
            {
                Id = id,
                IdUsuario = idUsuario
            };
            var filas = _productoServicio.Eliminar(producto);

            if (filas == 0)
                return BadRequest("No se eliminó ningún registro");

            // AUDITORÍA
            _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "DELETE", "Producto", $"Producto eliminado (baja lógica) ID: {id}");
            return Ok();
        }


        [HttpPost("{id}/presentaciones")]
        public IActionResult AgregarPresentacion(int id, [FromBody] SolicitudAgregarPresentacion dto)
        {
            var result = _productoServicio.AsociarNuevaPresentacion(
                id, dto.IdPresentacion, dto.FactorConversion, dto.PrecioVenta, dto.IdUsuario);
            if (result.IsFailure) return BadRequest(new { errores = result.Errors });

            // AUDITORÍA
            _bitacoraRepo.Registrar(GetIdUsuarioFromHeader(), "INSERT", "PresentacionProducto", $"Nueva presentación agregada al producto ID: {id}");
            return Ok();
        }
    }
}
