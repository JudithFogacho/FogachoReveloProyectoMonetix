using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FogachoReveloProyecto.Migrations
{
    /// <inheritdoc />
    public partial class CambiosBdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gasto_Usuario_UsuariosIdUsuario",
                table: "Gasto");

            migrationBuilder.DropIndex(
                name: "IX_Gasto_UsuariosIdUsuario",
                table: "Gasto");

            migrationBuilder.DropColumn(
                name: "UsuariosIdUsuario",
                table: "Gasto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuariosIdUsuario",
                table: "Gasto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Gasto_UsuariosIdUsuario",
                table: "Gasto",
                column: "UsuariosIdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Gasto_Usuario_UsuariosIdUsuario",
                table: "Gasto",
                column: "UsuariosIdUsuario",
                principalTable: "Usuario",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
