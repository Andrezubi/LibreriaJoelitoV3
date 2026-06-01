using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.Aplicacion.InterfacesExt;
using MicroServicioUsuarios.dominio.Entidades;
using MicroServicioUsuarios.dominio.EntidadesDeValor;
using MicroServicioUsuarios.dominio.Interfaces;
using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.Fabrica
{
    public sealed class FabricaUsuario : IUsuarioFabrica
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IContraHasher _hasher;

        public FabricaUsuario(IUsuarioRepositorio repositorio, IContraHasher hasher)
        {
            _repositorio = repositorio;
            _hasher = hasher;
        }

        public async Task<Resultado<(Usuario usuario, string passwordTemporal)>> CrearAsync(
            CrearUsuarioDto dto, int idUsuarioRegistrador)
        {
            var errores = new List<string>();

            // Validar cada campo con su Value Object 

            var nombreResult = NombrePersona.Crear(dto.Nombre, "Nombre");
            if (nombreResult.EsFallido) errores.Add(nombreResult.Error.Mensaje);

            var apPaternoResult = NombrePersona.Crear(dto.ApellidoPaterno, "Apellido paterno");
            if (apPaternoResult.EsFallido) errores.Add(apPaternoResult.Error.Mensaje);

            var apMaternoResult = NombrePersona.Crear(dto.ApellidoMaterno, "Apellido materno", esOpcional: true);
            if (apMaternoResult.EsFallido) errores.Add(apMaternoResult.Error.Mensaje);

            var ciResult = CarnetIdentidad.Crear(dto.Ci, dto.Complemento);
            if (ciResult.EsFallido) errores.Add(ciResult.Error.Mensaje);

            var emailResult = Email.Crear(dto.Email);
            if (emailResult.EsFallido) errores.Add(emailResult.Error.Mensaje);

            var telefonoResult = Telefono.Crear(dto.Telefono);
            if (telefonoResult.EsFallido) errores.Add(telefonoResult.Error.Mensaje);

            var direccionResult = Direccion.Crear(dto.DireccionDomicilio);
            if (direccionResult.EsFallido) errores.Add(direccionResult.Error.Mensaje);

            var fechaNacResult = FechaNacimiento.Crear(dto.FechaNacimiento);
            if (fechaNacResult.EsFallido) errores.Add(fechaNacResult.Error.Mensaje);

            var fechaIngResult = FechaIngreso.Crear(dto.FechaIngreso);
            if (fechaIngResult.EsFallido) errores.Add(fechaIngResult.Error.Mensaje);

            // Coherencia entre fechas (solo si ambas son válidas) ─
            if (fechaNacResult.EsExitoso && fechaIngResult.EsExitoso)
            {
                var coherencia = fechaIngResult.Valor.ValidarCoherenciaConNacimiento(fechaNacResult.Valor);
                if (coherencia.EsFallido) errores.Add(coherencia.Error.Mensaje);
            }

            //  Retornar todos los errores acumulados ─
            if (errores.Any())
                return Resultado.Fallido<(Usuario, string)>(
                    Error.Validacion(string.Join(" | ", errores)));

            //  Verificar CI único ─
            var ciExiste = await _repositorio.ExisteCiAsync(ciResult.Valor.Numero);
            if (ciExiste)
                return Resultado.Fallido<(Usuario, string)>(
                    Error.Conflicto($"Ya existe un usuario registrado con el CI {ciResult.Valor.ValorCompleto}."));

            //  Verificar email único 
            var emailExiste = await _repositorio.ExisteEmailAsync(emailResult.Valor.Valor);
            if (emailExiste)
                return Resultado.Fallido<(Usuario, string)>(
                    Error.Conflicto($"El email {emailResult.Valor.Valor} ya está registrado."));

            //  Generar nombre de usuario único ─
            var nombreUsuario = await GenerarNombreUsuarioUnicoAsync(
                dto.Nombre, dto.ApellidoPaterno);
            if (nombreUsuario.EsFallido)
                return Resultado.Fallido<(Usuario, string)>(nombreUsuario.Error);

            //  Generar y hashear contraseña temporal ─
            var passwordTemporal = GenerarPasswordSeguro();
            var hash = _hasher.Hashear(passwordTemporal);

            //  Construir la entidad (constructor interno) 
            var usuario = new Usuario(
                nombreUsuario: nombreUsuario.Valor.Valor,
                passwordHash: hash,
                rol: dto.Rol,
                nombre: nombreResult.Valor,
                apellidoPaterno: apPaternoResult.Valor,
                apellidoMaterno: apMaternoResult.Valor,
                ci: ciResult.Valor,
                fechaNacimiento: fechaNacResult.Valor,
                email: emailResult.Valor,
                direccion: direccionResult.Valor,
                telefono: telefonoResult.Valor,
                fechaIngreso: fechaIngResult.Valor,
                idUsuarioRegistrador: idUsuarioRegistrador);

            return Resultado.Exitoso((usuario, passwordTemporal));
        }

        //  Helpers privados 

        private async Task<Resultado<NombreUsuario>> GenerarNombreUsuarioUnicoAsync(
            string nombre, string apellido)
        {
            for (int sufijo = 0; sufijo <= 99; sufijo++)
            {
                var resultado = NombreUsuario.Generar(nombre, apellido, sufijo);
                if (resultado.EsFallido) return resultado;

                var existe = await _repositorio.ExisteNombreUsuarioAsync(resultado.Valor.Valor);
                if (!existe) return resultado;
            }

            return Resultado.Fallido<NombreUsuario>(
                Error.Conflicto("No se pudo generar un nombre de usuario único."));
        }

        private static string GenerarPasswordSeguro()
        {
            const string mayus = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minus = "abcdefghijklmnopqrstuvwxyz";
            const string nums = "0123456789";
            const string especial = "!@#$%&*?";
            const string todos = mayus + minus + nums + especial;

            var rng = new Random();
            var chars = new List<char>
        {
            mayus[rng.Next(mayus.Length)],
            minus[rng.Next(minus.Length)],
            nums[rng.Next(nums.Length)],
            especial[rng.Next(especial.Length)]
        };

            for (int i = 4; i < 12; i++)
                chars.Add(todos[rng.Next(todos.Length)]);

            return new string(chars.OrderBy(_ => rng.Next()).ToArray());
        }
    }

}
