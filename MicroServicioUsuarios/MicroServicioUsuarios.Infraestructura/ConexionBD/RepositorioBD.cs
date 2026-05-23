using MicroServicioUsuarios.dominio.Entidades;
using MicroServicioUsuarios.Infraestructura.ConexionBD.Configuraciones;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura.ConexionBD
{
    /// <summary>
    /// Singleton en el contenedor DI (registrado via AddDbContextPool).
    /// EF Core gestiona internamente el pool de conexiones — no instanciar manualmente.
    /// </summary>
    public sealed class RepositorioBD : DbContext
    {
        public RepositorioBD(DbContextOptions<RepositorioBD> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioConfig());
            base.OnModelCreating(modelBuilder);
        }
    }
}
