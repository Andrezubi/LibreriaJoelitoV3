using MicroServicioUsuarios.dominio.Entidades;
using MicroServicioUsuarios.dominio.Interfaces;
using MicroServicioUsuarios.Infraestructura.ConexionBD;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura.Persistencia
{
    public sealed class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly RepositorioBD _context;

        public UsuarioRepositorio(RepositorioBD context) => _context = context;

        public async Task<Usuario?> ObtenerPorIdAsync(int id) =>
            await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario) =>
            await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Estado);

        public async Task<Usuario?> ObtenerPorEmailAsync(string email) =>
            await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync() =>
            await _context.Usuarios
                .OrderBy(u => u.ApellidoPaterno)
                .ThenBy(u => u.Nombre)
                .ToListAsync();

        public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario) =>
            await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);

        public async Task<bool> ExisteCiAsync(string ci) =>
            await _context.Usuarios.AnyAsync(u => u.Ci == ci);

        public async Task<bool> ExisteEmailAsync(string email) =>
            await _context.Usuarios.AnyAsync(u => u.Email == email);

        public async Task AgregarAsync(Usuario usuario) =>
            await _context.Usuarios.AddAsync(usuario);

        public Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            return Task.CompletedTask;
        }

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }

}
