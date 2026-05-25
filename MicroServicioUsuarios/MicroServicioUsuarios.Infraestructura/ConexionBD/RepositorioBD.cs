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
        public DbSet<Bitacora> Bitacoras => Set<Bitacora>();

        public async Task InicializarEsquemaAsync()
        {
            await Database.EnsureCreatedAsync();

            // EnsureCreated no agrega tablas nuevas cuando la base ya existe.
            await Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS `Bitacora` (
                    `Id` INT NOT NULL AUTO_INCREMENT,
                    `IdUsuario` INT NOT NULL,
                    `Accion` VARCHAR(50) NOT NULL,
                    `Tabla` VARCHAR(100) NOT NULL,
                    `Fecha` DATETIME(6) NOT NULL,
                    `Descripcion` VARCHAR(500) NOT NULL,
                    CONSTRAINT `PK_Bitacora` PRIMARY KEY (`Id`)
                );
                """);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioConfig());
            modelBuilder.ApplyConfiguration<Bitacora>(new BitacoraConfig());
            base.OnModelCreating(modelBuilder);
        }
    }
}
