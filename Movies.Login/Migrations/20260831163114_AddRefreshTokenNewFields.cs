using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.Login.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedOnUtc",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedOnUtc",
                table: "RefreshTokens");
        }
    }
}
