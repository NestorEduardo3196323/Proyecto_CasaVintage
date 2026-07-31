using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasaVintage.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFotoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "foto",
                table: "usuarios",
                type: "varchar(255)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "foto",
                table: "usuarios");
        }
    }
}
