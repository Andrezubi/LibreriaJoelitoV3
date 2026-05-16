using System;

namespace MicroServicioProductos.Dominio.Modelos
{
    public class Producto
    {
        private string categoria;
        private decimal precio;
        private string tipoVenta;
        private decimal factorConversion;
        private int? idProductoBase;

        public int Id { get; set; }

        public int IdCategoria { get; set; }
        public int IdMarca { get; set; }
        public string Nombre { get; set; }

        public int Stock { get; set; }
        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaUltimaActualizacion { get; set; }

        public int? IdUsuario { get; set; }

        public Producto()
        {

        }
        public Producto(int id)// constructor para Delete
        {
            Id = id;
        }

        public Producto(int idCategoria,int idMarca, string nombre, int stock,int idUsuario)//constructor para insert
        {
            IdCategoria = idCategoria;
            IdMarca = idMarca;
            Nombre = nombre;
            Stock = stock;
            Estado = true;
            FechaRegistro = DateTime.Now;
            FechaUltimaActualizacion = DateTime.Now;
            IdUsuario = idUsuario;
        }
        public Producto(int id,int idCategoria, int idMarca, string nombre, int stock,int idUsuario)//constructor para update
        {
            Id = id;
            IdCategoria = idCategoria;
            IdMarca = idMarca;
            Nombre = nombre;
            Stock = stock;
            Estado = true;
            FechaRegistro = DateTime.Now;
            FechaUltimaActualizacion = DateTime.Now;
            IdUsuario = idUsuario;
        }

        public Producto(string categoria, string nombre, decimal precio, int stock, string tipoVenta, decimal factorConversion, int? idProductoBase)
        {
            this.categoria = categoria;
            Nombre = nombre;
            this.precio = precio;
            Stock = stock;
            this.tipoVenta = tipoVenta;
            this.factorConversion = factorConversion;
            this.idProductoBase = idProductoBase;
        }
    }
}
