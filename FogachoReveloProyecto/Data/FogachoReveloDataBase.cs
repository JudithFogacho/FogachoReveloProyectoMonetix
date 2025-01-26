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
        // Configurar enums para guardar como strings
        modelBuilder.Entity<Gasto>()
            .Property(g => g.Categorias)
            .HasConversion<string>();

        modelBuilder.Entity<Gasto>()
            .Property(g => g.Estados)
            .HasConversion<string>();

        // Configurar relación Usuario-Gasto
        modelBuilder.Entity<Usuario>()
    .HasMany(u => u.Gastos)
    .WithOne(g => g.Usuario)
    .HasForeignKey(g => g.IdUsuario)
    .OnDelete(DeleteBehavior.Restrict);
    }
}