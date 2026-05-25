using System;

namespace MicroServicioProductos.Dominio.Modelos
{
    public class Marca
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? PaginaWeb { get; set; }
        public string? Industria { get; set; }
        public bool Estado { get; set; } 
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaUltimaActualizacion { get; set; } // Puede ser NULL
        public int IdUsuario { get; set; } 

        public Marca() { }
        public Marca(int id) { Id = id; }

        public Marca(string nombre, string descripcion, string paginaWeb, string industria, int idUsuario)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            PaginaWeb = paginaWeb;
            Industria = industria;
            IdUsuario = idUsuario;
        }
    }
}