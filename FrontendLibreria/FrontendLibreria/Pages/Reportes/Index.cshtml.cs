using FrontendLibreria.Adaptadores.Reporte;
using FrontendLibreria.DTOs.Reportes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendLibreria.Pages.Reportes
{
    public class IndexModel : PageModel
    {
        private readonly IReporteAdapter _reporteAdapter;

        public IndexModel(IReporteAdapter reporteAdapter)
        {
            _reporteAdapter = reporteAdapter;
        }

        public List<BitacoraReporteDto> Bitacora { get; set; } = new();

        public async Task OnGetAsync()
        {
            Bitacora = await _reporteAdapter.ObtenerBitacoraAsync();
        }

        public async Task<IActionResult> OnPostGenerarVentasProductoAsync()
        {
            var request = new ReporteRequestDto(); // Valores por defecto
            var bytes = await _reporteAdapter.GenerarVentasPorProductoAsync(request);
            
            if (bytes == null || bytes.Length == 0) return RedirectToPage();

            return File(bytes, "application/pdf", "VentasPorProducto.pdf");
        }

        public async Task<IActionResult> OnPostGenerarResumenRecaudacionAsync()
        {
            var request = new ReporteRequestDto(); // Valores por defecto
            var bytes = await _reporteAdapter.GenerarResumenRecaudacionAsync(request);
            
            if (bytes == null || bytes.Length == 0) return RedirectToPage();

            return File(bytes, "application/pdf", "ResumenRecaudacion.pdf");
        }

        public async Task<IActionResult> OnPostVerComprobanteAsync(int idVenta)
        {
            var bytes = await _reporteAdapter.VerComprobanteVentaAsync(idVenta);
            
            if (bytes == null || bytes.Length == 0) return RedirectToPage();

            return File(bytes, "application/pdf");
        }
    }
}
