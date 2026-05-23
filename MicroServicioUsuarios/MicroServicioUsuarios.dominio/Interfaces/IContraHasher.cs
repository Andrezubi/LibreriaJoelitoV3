using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.dominio.Interfaces
{
    public interface IContraHasher
    {
        string Hashear(string password);
        bool Verificar(string password, string hash);
    }
}
