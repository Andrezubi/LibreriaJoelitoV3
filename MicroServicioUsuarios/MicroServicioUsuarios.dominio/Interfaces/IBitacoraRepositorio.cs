using System.Threading.Tasks;
using MicroServicioUsuarios.dominio.Entidades;

namespace MicroServicioUsuarios.dominio.Interfaces
{
    public interface IBitacoraRepositorio
    {
        Task RegistrarAsync(Bitacora bitacora);
        Task GuardarCambiosAsync();
    }
}
