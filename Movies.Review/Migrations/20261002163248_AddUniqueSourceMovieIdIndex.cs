using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.Review.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSourceMovieIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Movies_SourceMovieId",
                table: "Movies",
                column: "SourceMovieId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Movies_SourceMovieId",
                table: "Movies");
        }
    }
}
