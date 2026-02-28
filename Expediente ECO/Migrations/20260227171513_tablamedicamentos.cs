using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpedienteECO.Migrations
{
    public partial class tablamedicamentos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Creamos la tabla de Medicamentos
            migrationBuilder.CreateTable(
                name: "Medicamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.Id);
                });

            // 2. Creamos la tabla de Consultas (Sin la columna MedicamentoId que daba error)
            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sintomas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignosVitales = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnostico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tratamiento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultas_Estudiantes_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "Estudiantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 3. Creamos la tabla intermedia DetalleTratamientos
            migrationBuilder.CreateTable(
                name: "DetalleTratamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultaId = table.Column<int>(type: "int", nullable: false),
                    MedicamentoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleTratamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleTratamientos_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleTratamientos_Medicamentos_MedicamentoId",
                        column: x => x.MedicamentoId,
                        principalTable: "Medicamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Índices para mejorar la velocidad de búsqueda
            migrationBuilder.CreateIndex(
                name: "IX_Consultas_EstudianteId",
                table: "Consultas",
                column: "EstudianteId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTratamientos_ConsultaId",
                table: "DetalleTratamientos",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleTratamientos_MedicamentoId",
                table: "DetalleTratamientos",
                column: "MedicamentoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DetalleTratamientos");
            migrationBuilder.DropTable(name: "Consultas");
            migrationBuilder.DropTable(name: "Medicamentos");
        }
    }
}