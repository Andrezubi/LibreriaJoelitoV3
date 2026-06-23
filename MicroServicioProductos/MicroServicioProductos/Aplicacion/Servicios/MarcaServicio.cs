using MicroServicioProductos.Aplicacion.Results;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Dominio.Validadores;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroServicioProductos.Aplicacion.Servicios
{
    public class MarcaServicio
    {
        private readonly MarcaRepositorio marcaRepositorio;
        private readonly MarcaValidador marcaValidador;

        public MarcaServicio(MarcaRepositorio marcaRepositorio, MarcaValidador marcaValidador)
        {
            this.marcaRepositorio = marcaRepositorio;
            this.marcaValidador = marcaValidador;
        }

        public List<Marca> ObtenerTodo()
        {
            return marcaRepositorio.ObtenerTodo();
        }

        public Marca ObtenerPorId(int id)
        {
            return marcaRepositorio.ObtenerPorId(id);
        }

        public Result Insertar(Marca marca)
        {
            marca.Nombre = NormalizarTexto(marca.Nombre);
            marca.Industria = NormalizarTexto(marca.Industria);

            var validationResults = marcaValidador.Validar(marca);
            if (validationResults.Any())
            {
                var errors = validationResults
                    .Select(v => new ErrorValidacion(
                        v.MemberNames.FirstOrDefault() ?? "General",
                        v.ErrorMessage ?? "Error de validación"))
                    .ToList();

                return Result.Failure(errors);
            }

            if (marcaRepositorio.ExisteDuplicado(marca))
            {
                return Result.Failure(
                    "Nombre",
                    "Ya existe una marca registrada con este nombre.");
            }

            marcaRepositorio.Insertar(marca);
            return Result.Success();
        }

        public Result Actualizar(Marca marca)
        {
            marca.Nombre = NormalizarTexto(marca.Nombre);
            marca.Industria = NormalizarTexto(marca.Industria);
            var validationResults = marcaValidador.Validar(marca);
            if (validationResults.Any())
            {
                var errors = validationResults
                    .Select(v => new ErrorValidacion(
                        v.MemberNames.FirstOrDefault() ?? "General",
                        v.ErrorMessage ?? "Error de validación"))
                    .ToList();

                return Result.Failure(errors);
            }

            if (marcaRepositorio.ExisteDuplicado(marca))
            {
                return Result.Failure(
                    "Nombre",
                    "Ya existe una marca registrada con este nombre.");
            }

            marcaRepositorio.Actualizar(marca);
            return Result.Success();
        }

        public int Eliminar(Marca marca)
        {
            return marcaRepositorio.Eliminar(marca);
        }
        public string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = Regex.Replace(texto.Trim(), @"\s+", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        }
    }
}