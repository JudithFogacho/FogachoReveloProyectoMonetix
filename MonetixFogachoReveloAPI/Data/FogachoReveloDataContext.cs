using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MonetixFogachoReveloAPI.Data.Models;

namespace MonetixFogachoReveloAPI.Data;

public partial class FogachoReveloDataContext : DbContext
{
    public FogachoReveloDataContext()
    {
    }

    public FogachoReveloDataContext(DbContextOptions<FogachoReveloDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Gasto> Gastos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=FogachoRevelo.Data");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Gasto>(entity =>
        {
            entity.HasKey(e => e.IdGasto);

            entity.ToTable("Gasto");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
