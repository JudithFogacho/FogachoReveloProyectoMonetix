using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FogachoReveloProyecto.Models;

public class FogachoReveloDataBase : DbContext
{
    public FogachoReveloDataBase(DbContextOptions<FogachoReveloDataBase> options)
        : base(options)
    {
    }

    public DbSet<Gasto> Gasto { get; set; }
    public DbSet<Usuario> Usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar relación Usuario-Gasto
        modelBuilder.Entity<Usuario>()
            .HasMany(u => u.Gastos)
            .WithOne(g => g.Usuario)
            .HasForeignKey(g => g.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar enums para guardar como valores numéricos
        modelBuilder.Entity<Gasto>()
            .Property(g => g.Categorias)
            .HasColumnType("int");

        modelBuilder.Entity<Gasto>()
            .Property(g => g.Estados)
            .HasColumnType("int");

        // Configurar índices para mejor rendimiento
        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.IdUsuario);

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.Categorias);

        modelBuilder.Entity<Gasto>()
            .HasIndex(g => g.Estados);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();
        modelBuilder.Entity<Gasto>()
        .HasOne(g => g.Usuario)
        .WithMany(u => u.Gastos) // Si Usuario tiene una colección de Gastos
        .HasForeignKey(g => g.IdUsuario);
    }
}
