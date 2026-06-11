using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WallyBallBackend.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class MoveJugadorPersonalDataToPersonasService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Jugadores_Cedula",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "Cedula",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Jugadores");

            migrationBuilder.AddColumn<int>(
                name: "IdPersona",
                table: "Jugadores",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Jugadores_IdPersona",
                table: "Jugadores",
                column: "IdPersona",
                unique: true,
                filter: "[IdPersona] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Jugadores_IdPersona",
                table: "Jugadores");

            migrationBuilder.DropColumn(
                name: "IdPersona",
                table: "Jugadores");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Jugadores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cedula",
                table: "Jugadores",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaNacimiento",
                table: "Jugadores",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Jugadores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Jugadores",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Jugadores_Cedula",
                table: "Jugadores",
                column: "Cedula",
                unique: true);
        }
    }
}
