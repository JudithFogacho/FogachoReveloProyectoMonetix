﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FogachoReveloProyecto.Migrations
{
    /// <inheritdoc />
    public partial class BDD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
