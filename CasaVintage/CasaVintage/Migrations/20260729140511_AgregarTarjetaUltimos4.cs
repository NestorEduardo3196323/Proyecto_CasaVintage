using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasaVintage.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTarjetaUltimos4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tarjeta_ultimos4",
                table: "ventas",
                type: "varchar(4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tarjeta_ultimos4",
                table: "ventas");
        }
    }
}
