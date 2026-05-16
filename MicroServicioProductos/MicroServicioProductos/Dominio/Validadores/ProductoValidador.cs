
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using MicroServicioProductos.Dominio.Modelos;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MicroServicioProductos.Dominio.Validadores
{
    public  class ProductoValidador
    {
        public List<ValidationResult> ValidarProducto(Producto producto)
        {
            var errores = new List<ValidationResult>();

            ValidarNombre(producto.Nombre, errores);
            ValidarCategoria(producto.IdCategoria, errores);
            ValidarIdMarca(producto.IdMarca, errores);
            ValidarStock(producto.Stock, errores);
            

            return errores;
        }

        public void ValidarNombre(string nombre, List<ValidationResult> errores)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                errores.Add(new ValidationResult("El nombre es obligatorio.", new[] { "producto.Nombre" }));

            if (nombre.Length < 2)
                errores.Add(new ValidationResult("El nombre debe tener al menos 2 caracteres.", new[] { "producto.Nombre" }));

            if (nombre.Length > 150)
                errores.Add(new ValidationResult("El nombre no puede tener más de 150 caracteres.", new[] { "producto.Nombre" }));
            if (!Regex.IsMatch(nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                errores.Add(new ValidationResult(
                    "Nombre no puede contener caracteres inválidos y números.",
                    new[] { "producto.Nombre" }));
            }
        }

        public void ValidarCategoria(int idCategoria, List<ValidationResult> errores)
        {
            if (idCategoria <= 0)
                errores.Add(new ValidationResult("La categoría es obligatoria.", new[] { "producto.IdCategoria" }));
        }

        public void ValidarStock(int stock, List<ValidationResult> errores)
        {
            if (stock < 0)
                errores.Add(new ValidationResult("El stock no puede ser negativo.", new[] { "producto.Stock" }));
            if (stock > 100000000)
                errores.Add(new ValidationResult("El stock no puede sobrepasar los 100000000", new[] { "producto.Stock" }));
        }

        public void ValidarIdMarca(int idMarca, List<ValidationResult> errores)
        {
            if (idMarca <= 0)
                errores.Add(new ValidationResult("La marca es obligatoria.", new[] { "producto.IdMarca" }));
        }
    }
}
