using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.EntidadesDeValor;
using MicroServicioUsuarios.dominio.Interfaces;
using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.CasosDeUso
{
    /// <summary>
    /// En Servicio_Clientes era ObtenerTodo() retornando DataTable.
    /// Aquí retorna IEnumerable[UsuarioDto] tipado — sin exponer PasswordHash.
    /// El Select no debe desplegar el password (requisito de la rúbrica).
    /// </summary>
    public sealed class ObtenerUsuariosCasoDeUso
    {
        private readonly IUsuarioRepositorio _repo;
        public ObtenerUsuariosCasoDeUso(IUsuarioRepositorio repo) => _repo = repo;

        public async Task<Resultado<IEnumerable<UsuarioDto>>> EjecutarAsync()
        {
            var usuarios = await _repo.ObtenerTodosAsync();
            var dtos = usuarios.Select(u => new UsuarioDto(
                u.Id, u.NombreUsuario, u.NombreCompleto, u.CiCompleto,
                u.Email, u.Telefono, u.DireccionDomicilio, u.Rol,
                u.Estado, u.MustChangePassword,
                u.FechaNacimiento, u.FechaIngreso, u.FechaRegistro));
            return Resultado.Exitoso(dtos);
        }
    }

    /// <summary>
    /// En Servicio_Clientes era Actualizar() en UsuarioServicio con ValidadorEmpleado.
    /// Aquí cada campo se valida con su Value Object antes de actualizar la entidad.
    /// </summary>
    public sealed class ActualizarUsuarioCasoDeUso
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        public ActualizarUsuarioCasoDeUso(
            IUsuarioRepositorio usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public async Task<Resultado<UsuarioDto>> EjecutarAsync(
            int id, ActualizarUsuarioDto dto, int idModificador, string ipOrigen)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario is null)
                return Resultado.Fallido<UsuarioDto>(
                    Error.NoEncontrado($"No existe un usuario con ID {id}."));

            // Validar cada campo con su Value Object — acumulando errores
            var errores = new List<string>();

            var nombreR = NombrePersona.Crear(dto.Nombre, "Nombre");
            var apPaternoR = NombrePersona.Crear(dto.ApellidoPaterno, "Apellido paterno");
            var apMaternoR = NombrePersona.Crear(dto.ApellidoMaterno, "Apellido materno");
            var emailR = Email.Crear(dto.Email);
            var telefonoR = Telefono.Crear(dto.Telefono);
            var direccionR = Direccion.Crear(dto.DireccionDomicilio);

            if (nombreR.EsFallido) errores.Add(nombreR.Error.Mensaje);
            if (apPaternoR.EsFallido) errores.Add(apPaternoR.Error.Mensaje);
            if (apMaternoR.EsFallido) errores.Add(apMaternoR.Error.Mensaje);
            if (emailR.EsFallido) errores.Add(emailR.Error.Mensaje);
            if (telefonoR.EsFallido) errores.Add(telefonoR.Error.Mensaje);
            if (direccionR.EsFallido) errores.Add(direccionR.Error.Mensaje);

            if (errores.Any())
                return Resultado.Fallido<UsuarioDto>(
                    Error.Validacion(string.Join(" | ", errores)));

            usuario.Actualizar(
                nombreR.Valor, apPaternoR.Valor, apMaternoR.Valor,
                emailR.Valor, direccionR.Valor, telefonoR.Valor,
                dto.Rol, idModificador);

            await _usuarioRepo.ActualizarAsync(usuario);
            await _usuarioRepo.GuardarCambiosAsync();


            return Resultado.Exitoso(new UsuarioDto(
                usuario.Id, usuario.NombreUsuario, usuario.NombreCompleto,
                usuario.CiCompleto, usuario.Email, usuario.Telefono,
                usuario.DireccionDomicilio, usuario.Rol, usuario.Estado,
                usuario.MustChangePassword, usuario.FechaNacimiento,
                usuario.FechaIngreso, usuario.FechaRegistro));
        }
    }

}
