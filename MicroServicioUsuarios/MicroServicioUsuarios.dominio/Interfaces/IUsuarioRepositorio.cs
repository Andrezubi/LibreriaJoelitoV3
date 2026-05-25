using MicroServicioUsuarios.dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.Interfaces
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
        Task<bool> ExisteCiAsync(string ci);
        Task<bool> ExisteEmailAsync(string email);
        Task AgregarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task GuardarCambiosAsync();
    }
}
