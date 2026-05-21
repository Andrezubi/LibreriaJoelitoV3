using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServicioProveedores.Aplicacion.DTOs
{
    public class RegistrarProveedorDto
    {
        public string Nombre { get; set; }
        public int Nit { get; set; }
        public int TelefonoContacto { get; set; }
        public string? Descripcion { get; set; }
        public string Direccion { get; set; }
        public int IdUsuario { get; set; } 
    }
}
