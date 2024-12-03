using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FogachoReveloProyecto.Migrations
{
    /// <inheritdoc />
    public partial class CambiosDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Gasto",
                columns: table => new
                {
                    IdGasto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaRegristo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Categorias = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Valor = table.Column<double>(type: "float", nullable: false),
                    ValorPagado = table.Column<double>(type: "float", nullable: false),
                    Estados = table.Column<int>(type: "int", nullable: false),
                    UsuariosIdUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gasto", x => x.IdGasto);
                    table.ForeignKey(
                        name: "FK_Gasto_Usuario_UsuariosIdUsuario",
                        column: x => x.UsuariosIdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_UsuariosIdUsuario",
                table: "Gasto",
                column: "UsuariosIdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gasto");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
