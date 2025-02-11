﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonetixFogachoReveloAPI.Data;

#nullable disable

namespace MonetixFogachoReveloAPI.Migrations
{
    [DbContext(typeof(FogachoReveloDataContext))]
    [Migration("20250129014904_FogachoReveloData")]
    partial class FogachoReveloData
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MonetixFogachoReveloAPI.Data.Models.Gasto", b =>
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

                    b.Property<int>("IdUsuario")
                        .HasColumnType("int");

                    b.Property<double>("Valor")
                        .HasColumnType("float");

                    b.Property<double>("ValorPagado")
                        .HasColumnType("float");

                    b.HasKey("IdGasto");

                    b.HasIndex("IdUsuario");

                    b.ToTable("Gasto", (string)null);
                });

            modelBuilder.Entity("MonetixFogachoReveloAPI.Data.Models.Usuario", b =>
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

                    b.ToTable("Usuario", (string)null);
                });

            modelBuilder.Entity("MonetixFogachoReveloAPI.Data.Models.Gasto", b =>
                {
                    b.HasOne("MonetixFogachoReveloAPI.Data.Models.Usuario", "Usuario")
                        .WithMany("Gastos")
                        .HasForeignKey("IdUsuario")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Usuario");
                });

            modelBuilder.Entity("MonetixFogachoReveloAPI.Data.Models.Usuario", b =>
                {
                    b.Navigation("Gastos");
                });
#pragma warning restore 612, 618
        }
    }
}
