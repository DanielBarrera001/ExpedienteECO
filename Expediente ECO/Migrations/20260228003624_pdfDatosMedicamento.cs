using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpedienteECO.Migrations
{
    /// <inheritdoc />
    public partial class pdfDatosMedicamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadUsada",
                table: "Consultas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MedicamentoId",
                table: "Consultas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_MedicamentoId",
                table: "Consultas",
                column: "MedicamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Medicamentos_MedicamentoId",
                table: "Consultas",
                column: "MedicamentoId",
                principalTable: "Medicamentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Medicamentos_MedicamentoId",
                table: "Consultas");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_MedicamentoId",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "CantidadUsada",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "MedicamentoId",
                table: "Consultas");
        }
    }
}
