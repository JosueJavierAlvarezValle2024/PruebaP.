using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Prueba3._0.Models;

namespace Prueba3._0.Data;

public class ApplicationDbContext : IdentityDbContext<Usuario>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<HistorialVersion> HistorialVersiones => Set<HistorialVersion>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Documento>()
            .HasIndex(d => new { d.Titulo, d.Version })
            .IsUnique();

        builder.Entity<Documento>()
            .Property(d => d.FechaCreacion)
            .HasDefaultValueSql("getutcdate()");

        builder.Entity<Auditoria>()
            .Property(a => a.Fecha)
            .HasDefaultValueSql("getutcdate()");

        // SQL Server no permite múltiples rutas de cascada hacia la misma tabla.
        // Se deja CASCADE solo en la FK principal (Usuario → Documento),
        // y Restrict en las FK secundarias de HistorialVersion y Auditoria.
        builder.Entity<HistorialVersion>()
            .HasOne(h => h.Documento)
            .WithMany(d => d.HistorialVersiones)
            .HasForeignKey(h => h.DocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HistorialVersion>()
            .HasOne(h => h.Usuario)
            .WithMany()
            .HasForeignKey(h => h.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Auditoria>()
            .HasOne(a => a.Documento)
            .WithMany(d => d.Auditorias)
            .HasForeignKey(a => a.DocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Auditoria>()
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
