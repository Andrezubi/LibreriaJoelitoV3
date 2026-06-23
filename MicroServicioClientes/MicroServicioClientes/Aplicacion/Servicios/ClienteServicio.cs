using MicroServicioClientes.Aplicacion.Results;
using MicroServicioClientes.Dominio.Modelos;
using MicroServicioClientes.Dominio.Validadores;
using MicroServicioClientes.Infrestructura.Persistencia.FactoriaProductos;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MicroServicioClientes.Aplicacion.Servicios
{
    public class ClienteServicio
    {
        private readonly ClienteRepositorio clienteRepositorio;
        private readonly ClienteValidador clienteValidador;

        public ClienteServicio(ClienteRepositorio clienteRepositorio, ClienteValidador clienteValidador)
        {
            this.clienteRepositorio = clienteRepositorio;
            this.clienteValidador = clienteValidador;
        }

        public List<Cliente> ObtenerTodo()
        {
            return clienteRepositorio.ObtenerTodo();
        }

        public Cliente ObtenerPorId(int id)
        {
            return clienteRepositorio.ObtenerPorId(id);
        }

        public Cliente? ObtenerPorCi(string ci)
        {
            var cliente = clienteRepositorio.ObtenerPorCi(ci);
            // ObtenerPorCi devuelve new Cliente() si no encuentra, chequeamos Id
            return cliente.Id > 0 ? cliente : null;
        }

        public List<Cliente> ObtenerSimilarCi(string ci)
        {
            return clienteRepositorio.ObtenerSimilarCi(ci);
        }

        public Result<int> Insertar(Cliente cliente)
        {
            cliente.RazonSocial=NormalizarTexto(cliente.RazonSocial);
            var validationResults = clienteValidador.Validar(cliente);
            if (validationResults.Any())
            {
                var errors = validationResults
                    .Select(v => $"{v.ErrorMessage}")
                    .ToList();
                return Result<int>.Failure(errors);
            }

            if (clienteRepositorio.ExisteDuplicado(cliente))
                return Result<int>.Failure("Ya existe un cliente con este CI y Complemento.");

            int idGenerado = clienteRepositorio.Insertar(cliente);
            return Result<int>.Success(idGenerado);
        }

        public Result Actualizar(Cliente cliente)
        {
            var validationResults = clienteValidador.Validar(cliente);
            if (validationResults.Any())
            {
                var errors = validationResults
                    .Select(v => $"{v.ErrorMessage}")
                    .ToList();
                return Result.Failure(errors);
            }

            if (clienteRepositorio.ExisteDuplicado(cliente))
                return Result.Failure("Ya existe un cliente con este CI y Complemento.");

            clienteRepositorio.Actualizar(cliente);
            return Result.Success();
        }

        public int Eliminar(Cliente cliente)
        {
            return clienteRepositorio.Eliminar(cliente);
        }
        public string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = Regex.Replace(texto.Trim(), @"\s+", " ");
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
        }
    }
}