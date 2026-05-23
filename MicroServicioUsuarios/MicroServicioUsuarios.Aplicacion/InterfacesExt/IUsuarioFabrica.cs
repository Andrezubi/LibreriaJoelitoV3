using MicroServicioUsuarios.Aplicacion.DTOs;
using MicroServicioUsuarios.dominio.Entidades;
using MicroServicioUsuarios.dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Aplicacion.InterfacesExt
{
    public interface IUsuarioFabrica
    {
        Task<Resultado<(Usuario usuario, string passwordTemporal)>> CrearAsync(CrearUsuarioDto dto, string creadoPor);
    }
}
