
using Microsoft.Data.SqlClient;
using MicroServicioProductos.Infraestructura.Persistencia;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroServicioProductos.Dominio.Validadores;

public static class ExtraValidador
{


    public static List<ValidationResult> ValidarNombreCategoria(string nombre)
    {
        var errores = new List<ValidationResult>();
        var bd = RepositorioBD.Instancia;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(new ValidationResult(
                "El nombre de la categoría es obligatorio.",
                new[] { "IdCategoria" }));
            return errores;
        }
        if (ContieneCaracteresInvalidos(nombre))
        {
            errores.Add(new ValidationResult(
                "Nombre no puede contener Caracteres Invalidos.",
                new[] { "IdCategoria" }));
        }
        // 2. Longitud mínima
        if (nombre.Length < 3)
        {
            errores.Add(new ValidationResult(
                "El nombre debe tener al menos 3 caracteres.",
                new[] { "IdCategoria" }));
        }

        // 3. Longitud máxima
        if (nombre.Length > 100)
        {
            errores.Add(new ValidationResult(
                "El nombre no puede tener más de 100 caracteres.",
                new[] { "IdCategoria" }));
        }

        // 4. Validar duplicado en BD
        string query = @"
                SELECT 1 
                FROM categoria
                WHERE LOWER(TRIM(Nombre)) = LOWER(TRIM(@nombre))
                LIMIT 1;
            ";

        using (var cmd = new SqlCommand(query))
        {
            cmd.Parameters.AddWithValue("@nombre", nombre);


            using (var reader = bd.ExecuteReader(cmd))
            {
                if (reader.HasRows)
                {
                    errores.Add(new ValidationResult(
                        "Ya existe una categoria con ese nombre.",
                        new[] { "IdCategoria" }));
                }
            }
        }
        return errores;
    }
    public static List<ValidationResult> ValidarNombreMarca(string nombre)
    {
        var errores = new List<ValidationResult>();
        var bd = RepositorioBD.Instancia;

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(new ValidationResult(
                "El nombre de la marca es obligatorio.",
                new[] { "IdMarca" }));
            return errores;
        }
        if (ContieneCaracteresInvalidos(nombre))
        {
            errores.Add(new ValidationResult(
                "El nombre no puede contener Caracteres Invalidos.",
                new[] { "IdMarca" }));
        }
        // 2. Longitud mínima
        if (nombre.Length < 3)
        {
            errores.Add(new ValidationResult(
                "El nombre debe tener al menos 3 caracteres.",
                new[] { "IdMarca" }));
        }

        // 3. Longitud máxima
        if (nombre.Length > 100)
        {
            errores.Add(new ValidationResult(
                "El nombre no puede tener más de 100 caracteres.",
                new[] { "IdMarca" }));
        }

        // 4. Validar duplicado en BD
        string query = @"
                SELECT 1 
                FROM marca 
                WHERE LOWER(TRIM(Nombre)) = LOWER(TRIM(@nombre))
                LIMIT 1;
            ";

        using (var cmd = new SqlCommand(query))
        {
            cmd.Parameters.AddWithValue("@nombre", nombre);

            using (var reader = bd.ExecuteReader(cmd))
            {
                if (reader.HasRows)
                {
                    errores.Add(new ValidationResult(
                        "Ya existe una marca con ese nombre.",
                        new[] { "IdMarca" }));
                }
            }
        }
        return errores;
    }

    public static bool ContieneCaracteresInvalidos(string texto)
    {
        return !Regex.IsMatch(texto, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s\-\.\&\(\),\/]+$");
    }

}
