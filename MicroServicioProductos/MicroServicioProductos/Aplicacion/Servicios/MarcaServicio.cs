using MicroServicioProductos.Dominio.Validadores;
using MicroServicioProductos.Aplicacion.Results;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;

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
            var validationResults = marcaValidador.Validar(marca);
            if (validationResults.Any())
            {
                var errors = validationResults
                    .Select(v => $"{v.ErrorMessage}")
                    .ToList();
                return Result.Failure(errors);
            }

            if (marcaRepositorio.ExisteDuplicado(marca))
                return Result.Failure("Ya existe una marca registrada con este nombre.");

            marcaRepositorio.Insertar(marca);
            return Result.Success();
        }

        public Result Actualizar(Marca marca)
        {
            var validationResults = marcaValidador.Validar(marca);
            if (validationResults.Any())
            {
                var errors = validationResults
                    .Select(v => $"{v.ErrorMessage}")
                    .ToList();
                return Result.Failure(errors);
            }

            if (marcaRepositorio.ExisteDuplicado(marca))
                return Result.Failure("Ya existe una marca registrada con este nombre.");

            marcaRepositorio.Actualizar(marca);
            return Result.Success();
        }

        public int Eliminar(Marca marca)
        {
            return marcaRepositorio.Eliminar(marca);
        }
    }
}