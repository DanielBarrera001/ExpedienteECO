using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpedienteECO.Migrations
{
    /// <inheritdoc />
    public partial class detallesMedicamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Indicaciones",
                table: "Medicamentos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Presentacion",
                table: "Medicamentos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Indicaciones",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "Presentacion",
                table: "Medicamentos");
        }
    }
}
