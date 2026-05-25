using MicroServicioUsuarios.dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroServicioUsuarios.Infraestructura.ConexionBD.Configuraciones
{
    public sealed class BitacoraConfig : IEntityTypeConfiguration<Bitacora>
    {
        public void Configure(EntityTypeBuilder<Bitacora> builder)
        {
            builder.ToTable("Bitacora");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            builder.Property(b => b.IdUsuario)
                .HasColumnName("IdUsuario")
                .IsRequired();

            builder.Property(b => b.Accion)
                .HasColumnName("Accion")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(b => b.Tabla)
                .HasColumnName("Tabla")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.Fecha)
                .HasColumnName("Fecha")
                .IsRequired();

            builder.Property(b => b.Descripcion)
                .HasColumnName("Descripcion")
                .HasMaxLength(500)
                .IsRequired();
        }
    }
}
