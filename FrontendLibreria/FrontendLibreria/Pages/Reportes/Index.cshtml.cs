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

        [BindProperty]
        public DateTime? FechaDesde { get; set; }

        [BindProperty]
        public DateTime? FechaHasta { get; set; }

        [BindProperty]
        public string OrdenPor { get; set; } = "producto";

        [BindProperty]
        public bool Descendente { get; set; }

        public string? MensajeError { get; set; }

        public void OnGet()
        {
            EstablecerFechasPorDefecto();
        }

        public async Task<IActionResult> OnPostGenerarVentasProductoAsync()
        {
            if (!ValidarRangoFechas())
            {
                return Page();
            }

            var request = CrearRequestReporte();
            var bytes = await _reporteAdapter.GenerarVentasPorProductoAsync(request);
            
            if (bytes == null || bytes.Length == 0)
            {
                MensajeError = "No se pudo generar el reporte de ventas por producto.";
                return Page();
            }

            return File(bytes, "application/pdf", "VentasPorProducto.pdf");
        }

        public async Task<IActionResult> OnPostGenerarResumenRecaudacionAsync()
        {
            if (!ValidarRangoFechas())
            {
                return Page();
            }

            var request = CrearRequestReporte();
            var bytes = await _reporteAdapter.GenerarResumenRecaudacionAsync(request);
            
            if (bytes == null || bytes.Length == 0)
            {
                MensajeError = "No se pudo generar el resumen de recaudación.";
                return Page();
            }

            return File(bytes, "application/pdf", "ResumenRecaudacion.pdf");
        }

        private ReporteRequestDto CrearRequestReporte()
        {
            return new ReporteRequestDto
            {
                FechaDesde = FechaDesde,
                FechaHasta = FechaHasta,
                IdUsuario = ObtenerIdUsuario(),
                Usuario = ObtenerNombreUsuario(),
                OrdenPor = string.IsNullOrWhiteSpace(OrdenPor) ? "producto" : OrdenPor,
                Descendente = Descendente
            };
        }

        private void EstablecerFechasPorDefecto()
        {
            FechaDesde ??= DateTime.Today.AddDays(-5);
            FechaHasta ??= DateTime.Today;
        }

        private bool ValidarRangoFechas()
        {
            if (FechaDesde.HasValue &&
                FechaHasta.HasValue &&
                FechaDesde.Value.Date > FechaHasta.Value.Date)
            {
                MensajeError = "La fecha desde no puede ser mayor que la fecha hasta.";
                return false;
            }

            return true;
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
