using MicroServicioUsuarios.dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroServicioUsuarios.Infraestructura.ConexionBD.Configuraciones
{
    public sealed class UsuarioConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("usuarios");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(u => u.NombreUsuario)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();
            builder.HasIndex(u => u.NombreUsuario).IsUnique();

            builder.Property(u => u.PasswordHash)
                .HasColumnName("password")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.MustChangePassword)
                .HasColumnName("must_change_password");

            builder.Property(u => u.Rol)
                .HasColumnName("rol")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.Estado)
                .HasColumnName("estado");

            // ── Datos personales ─────────────────────────────────────────────
            builder.Property(u => u.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.ApellidoPaterno)
                .HasColumnName("apellido_paterno")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.ApellidoMaterno)
                .HasColumnName("apellido_materno")
                .HasMaxLength(100);

            builder.Property(u => u.Ci)
                .HasColumnName("ci")
                .HasMaxLength(10)
                .IsRequired();
            builder.HasIndex(u => u.Ci).IsUnique();

            builder.Property(u => u.Complemento)
                .HasColumnName("complemento")
                .HasMaxLength(10);

            builder.Property(u => u.FechaNacimiento)
                .HasColumnName("fecha_nacimiento")
                .HasColumnType("date");

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(150)
                .IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.DireccionDomicilio)
                .HasColumnName("direccion_domicilio")
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(u => u.FechaIngreso)
                .HasColumnName("fecha_ingreso")
                .HasColumnType("date");

            // ── Auditoría ────────────────────────────────────────────────────
            builder.Property(u => u.FechaRegistro)
                .HasColumnName("fecha_registro");

            builder.Property(u => u.FechaUltimaActualizacion)
                .HasColumnName("fecha_ultima_actualizacion");

            builder.Property(u => u.IdUsuario)
                .HasColumnName("id_usuario");
        }
    }

}
