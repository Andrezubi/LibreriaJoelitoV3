using FrontendLibreria.Adaptadores.Producto;
using FrontendLibreria.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Productos
{

    // Pages/Productos/MostrarProductos.cshtml.cs
    //[Authorize(Roles = "Administrador,Empleado")]
    public class IndiceProductosModel : PageModel
    {
        private readonly IAdaptadorProducto _productoAdapter;

        public IndiceProductosModel(IAdaptadorProducto productoAdapter)
        {
            _productoAdapter = productoAdapter;
        }

        // Ya no son DataTables, son listas de DTOs
        public List<ProductoDto> Productos { get; set; } = new();
        public List<CategoriaDto> Categorias { get; set; } = new();
        public List<MarcaDto> Marcas { get; set; } = new();
        public List<PresentacionDto> Presentaciones { get; set; } = new();

        [BindProperty] public int IdProductoSeleccionado { get; set; }
        [BindProperty] public int IdPresentacionSeleccionada { get; set; }
        [BindProperty] public int FactorConversion { get; set; } = 1;
        [BindProperty] public decimal PrecioVenta { get; set; }

        [TempData] public string? MensajeExito { get; set; }

        public async Task OnGetAsync()
        {

            Productos = await _productoAdapter.GetAllAsync();
            Categorias =  await _productoAdapter.GetCategoriasAsync();
            Marcas = await _productoAdapter.GetMarcasAsync();   
            Presentaciones = await _productoAdapter.GetPresentacionesAsync();

        }

        public async Task<IActionResult> OnPostAgregarPresentacionAsync()
        {
            int idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            var result = await _productoAdapter.AgregarPresentacionAsync(new SolicitudAgregarPresentacion
            {
                IdProducto = IdProductoSeleccionado,
                IdPresentacion = IdPresentacionSeleccionada,
                FactorConversion = FactorConversion,
                PrecioVenta = PrecioVenta,
                IdUsuario = idUsuario
            });

            if (!result.Success)
            {
                TempData["MensajeError"] = string.Join(", ", result.Errors);
                return RedirectToPage();
            }

            TempData["MensajeExito"] = "Presentación agregada exitosamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            int idUsuario = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            var resultado = await _productoAdapter.DeleteAsync(id, idUsuario);

            if (resultado.Success)
                TempData["MensajeExito"] = "El producto fue eliminado correctamente.";
            else
                TempData["MensajeError"] = "No se pudo eliminar el producto.";

            return RedirectToPage();
        }

        public async Task<JsonResult> OnPostUpdateAsync([FromForm] ProductoDto producto)
        {
            //producto.IdUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");
            //var result = await _productoAdapter.UpdateAsync(producto);

            //TempData["MensajeExito"] = "El producto fue editado correctamente.";
            //return new JsonResult(new { success = true });

            producto.IdUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "1");

            var result = await _productoAdapter.UpdateAsync(producto);

            if (!result.Success)
            {
                return new JsonResult(new
                {
                    success = false,
                    errores = result.Errors
                });
            }

            TempData["MensajeExito"] = "El producto fue editado correctamente.";

            return new JsonResult(new
            {
                success = true
            });

        }
    }
}   
