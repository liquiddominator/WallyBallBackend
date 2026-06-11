using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WallyBallBackend.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSecurityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangedAtUtc",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordChangedAtUtc",
                table: "Usuarios");
        }
    }
}
