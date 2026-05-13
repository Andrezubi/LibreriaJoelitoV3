using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendLibreria.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            var claim = User.FindFirst("MustChangePassword");
            Console.WriteLine($">>> MustChangePassword claim: '{claim?.Value}' tipo: '{claim?.Type}'");
        }
    }
}
