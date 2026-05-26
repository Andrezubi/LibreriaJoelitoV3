using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.DTOs
{
    public record CrearUsuarioDto(
    string Nombre,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string Ci,
    string? Complemento,
    DateOnly FechaNacimiento,
    string Email,
    string? DireccionDomicilio,
    string Telefono,
    DateOnly FechaIngreso,
    string Rol
);

    public record ActualizarUsuarioDto(
        string Nombre,
        string ApellidoPaterno,
        string? ApellidoMaterno,
        string Email,
        string DireccionDomicilio,
        string Telefono,
        string Rol
    );

    /// <summary>Select — nunca expone PasswordHash.</summary>
    public record UsuarioDto(
        int Id,
        string NombreUsuario,
        string NombreCompleto,
        string CiCompleto,
        string Email,
        string Telefono,
        string DireccionDomicilio,
        string Rol,
        bool Estado,
        bool MustChangePassword,
        DateOnly FechaNacimiento,
        DateOnly FechaIngreso,
        DateTime FechaRegistro
    );

    public record LoginRequestDto(
        string NombreUsuario,
        string Password
    );

    public record LoginResponseDto(
        string Token,
        string NombreUsuario,
        string NombreCompleto,
        string Rol,
        bool MustChangePassword
    );

    public record CambiarPasswordDto(
        string PasswordActual,
        string NuevoPassword,
        string ConfirmarPassword
    );

}
