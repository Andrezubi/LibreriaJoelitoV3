using MicroServicioUsuarios.dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MicroServicioUsuarios.Infraestructura.ConexionBD.Configuraciones
{
    public sealed class BitacoraConfig : IEntityTypeConfiguration<Bitacora>
    {
        public void Configure(EntityTypeBuilder<Bitacora> builder)
        {
            builder.ToTable("bitacoras");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(b => b.Usuario)
                .HasColumnName("usuario")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.Accion)
                .HasColumnName("accion")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(b => b.Modulo)
                .HasColumnName("modulo")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(b => b.Ip)
                .HasColumnName("ip")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(b => b.Detalle)
                .HasColumnName("detalle")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(b => b.Fecha)
                .HasColumnName("fecha");
        }
    }
}
