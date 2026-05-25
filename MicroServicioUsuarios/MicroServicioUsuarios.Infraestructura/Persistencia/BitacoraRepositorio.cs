using MicroServicioUsuarios.dominio.Entidades;
using MicroServicioUsuarios.dominio.Interfaces;
using MicroServicioUsuarios.Infraestructura.ConexionBD;

namespace MicroServicioUsuarios.Infraestructura.Persistencia
{
    public sealed class BitacoraRepositorio : IBitacoraRepositorio
    {
        private readonly RepositorioBD _context;

        public BitacoraRepositorio(RepositorioBD context) => _context = context;

        public async Task RegistrarAsync(Bitacora bitacora) =>
            await _context.Bitacoras.AddAsync(bitacora);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
