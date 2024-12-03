﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FogachoReveloProyecto.Migrations
{
    [DbContext(typeof(FogachoReveloDataBase))]
    [Migration("20241016185254_CambiosDB")]
    partial class CambiosDB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FogachoReveloProyecto.Models.Gasto", b =>
                {
                    b.Property<int>("IdGasto")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdGasto"));

                    b.Property<int>("Categorias")
                        .HasColumnType("int");

                    b.Property<string>("Descripcion")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Estados")
                        .HasColumnType("int");

                    b.Property<DateTime>("FechaFinal")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("FechaRegristo")
                        .HasColumnType("datetime2");

                    b.Property<int>("UsuariosIdUsuario")
                        .HasColumnType("int");

                    b.Property<double?>("Valor")
                        .IsRequired()
                        .HasColumnType("float");

                    b.Property<double?>("ValorPagado")
                        .IsRequired()
                        .HasColumnType("float");

                    b.HasKey("IdGasto");

                    b.HasIndex("UsuariosIdUsuario");

                    b.ToTable("Gasto");
                });

            modelBuilder.Entity("FogachoReveloProyecto.Models.Usuario", b =>
                {
                    b.Property<int>("IdUsuario")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdUsuario"));

                    b.Property<string>("Apellido")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdUsuario");

                    b.ToTable("Usuario");
                });

            modelBuilder.Entity("FogachoReveloProyecto.Models.Gasto", b =>
                {
                    b.HasOne("FogachoReveloProyecto.Models.Usuario", "Usuarios")
                        .WithMany("Gastos")
                        .HasForeignKey("UsuariosIdUsuario")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuarios");
                });

            modelBuilder.Entity("FogachoReveloProyecto.Models.Usuario", b =>
                {
                    b.Navigation("Gastos");
                });
#pragma warning restore 612, 618
        }
    }
}
