using FrontendLibreria.Adaptadores.Reporte;
using FrontendLibreria.DTOs.Reportes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FrontendLibreria.Pages.Reportes
{
    public class IndexModel : PageModel
    {
        private readonly IReporteAdapter _reporteAdapter;

        public IndexModel(IReporteAdapter reporteAdapter)
        {
            _reporteAdapter = reporteAdapter;
        }

        public async Task<IActionResult> OnPostGenerarVentasProductoAsync()
        {
            var request = CrearRequestReporte();
            var bytes = await _reporteAdapter.GenerarVentasPorProductoAsync(request);
            
            if (bytes == null || bytes.Length == 0) return RedirectToPage();

            return File(bytes, "application/pdf", "VentasPorProducto.pdf");
        }

        public async Task<IActionResult> OnPostGenerarResumenRecaudacionAsync()
        {
            var request = CrearRequestReporte();
            var bytes = await _reporteAdapter.GenerarResumenRecaudacionAsync(request);
            
            if (bytes == null || bytes.Length == 0) return RedirectToPage();

            return File(bytes, "application/pdf", "ResumenRecaudacion.pdf");
        }

        private ReporteRequestDto CrearRequestReporte()
        {
            return new ReporteRequestDto
            {
                IdUsuario = ObtenerIdUsuario(),
                Usuario = ObtenerNombreUsuario()
            };
        }

        private int? ObtenerIdUsuario()
        {
            var idClaim = User.FindFirst("IdUsuario")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(idClaim, out var idUsuario) ? idUsuario : null;
        }

        private string ObtenerNombreUsuario()
        {
            return User.FindFirst("NombreCompleto")?.Value
                ?? User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.Identity?.Name
                ?? "Sistema";
        }
    }
}
