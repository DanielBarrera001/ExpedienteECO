using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ExpedienteECO.Entidades;

namespace ExpedienteECO.Controllers;

public class ReportesController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string ColorEscuela = "#003366";
    private readonly string _logoPath;
    private readonly string _selloPath;

    public ReportesController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
        _logoPath = Path.Combine(env.WebRootPath, "img", "logo_escuela.png");
        _selloPath = Path.Combine(env.WebRootPath, "img", "sello_clinica.png");
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> ImprimirConsulta(int id)
    {
        var consulta = await _context.Consultas
            .Include(c => c.Estudiante)
            .Include(c => c.Medicamento)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta == null) return NotFound();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Row(row =>
                {
                    if (System.IO.File.Exists(_logoPath))
                        row.ConstantItem(75).Image(_logoPath);
                    else
                        row.ConstantItem(75).Placeholder();

                    row.RelativeItem().PaddingLeft(10).Column(col =>
                    {
                        col.Item().Text("ESCUELA CRISTIANA OASIS").FontSize(16).ExtraBold().FontColor(ColorEscuela);
                        col.Item().Text("CLÍNICA ESCOLAR - DEPARTAMENTO MÉDICO").FontSize(11).SemiBold().FontColor(Colors.Grey.Medium);
                        col.Item().Text("Teléfono: 2222-0000").FontSize(9);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("HOJA DE ATENCIÓN").FontSize(10).Bold().FontColor(ColorEscuela);
                        col.Item().Text($"N°: #{consulta.Id:D6}");
                        col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")).FontSize(8);
                    });
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // BLOQUE ESTUDIANTE
                    col.Item().Background(ColorEscuela).Padding(5).Text("INFORMACIÓN DEL ESTUDIANTE").FontColor(Colors.White).Bold();
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
                    {
                        var fotoRuta = !string.IsNullOrEmpty(consulta.Estudiante.FotoPath)
                            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", consulta.Estudiante.FotoPath.TrimStart('/')) : "";

                        if (System.IO.File.Exists(fotoRuta)) row.ConstantItem(85).Image(fotoRuta);
                        else row.ConstantItem(85).Background(Colors.Grey.Lighten3).AlignCenter().AlignMiddle().Text("SIN FOTO").FontSize(8);

                        row.RelativeItem().PaddingLeft(15).Column(c =>
                        {
                            c.Item().Text(consulta.Estudiante.NombreCompleto).FontSize(14).Bold();
                            c.Item().Text($"Grado: {consulta.Estudiante.Grado} | Sección: {consulta.Estudiante.Seccion}");
                            c.Item().PaddingTop(5).Text($"Fecha de Atención: {consulta.FechaHora:dd/MM/yyyy hh:mm tt}");
                        });
                    });

                    // BLOQUE EVALUACIÓN
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem(1).Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("SIGNOS VITALES").FontSize(9).Bold().FontColor(ColorEscuela);
                            c.Item().Text(consulta.SignosVitales);
                        });
                        row.ConstantItem(15);
                        row.RelativeItem(2).Column(c =>
                        {
                            c.Item().Text("MOTIVO DE CONSULTA").FontSize(9).Bold().FontColor(ColorEscuela);
                            c.Item().Text(consulta.Motivo);
                            c.Item().PaddingTop(10).Text("DIAGNÓSTICO MÉDICO").FontSize(9).Bold().FontColor(ColorEscuela);
                            c.Item().Text(consulta.Diagnostico).FontSize(11);
                        });
                    });

                    // BLOQUE TRATAMIENTO
                    col.Item().PaddingTop(20).Background(Colors.Grey.Lighten5).Border(0.5f).Padding(10).Column(c =>
                    {
                        c.Item().Text("PLAN DE TRATAMIENTO").FontSize(9).Bold().FontColor(Colors.Green.Darken3);
                        c.Item().Text(consulta.Tratamiento);

                        if (consulta.MedicamentoId != null)
                        {
                            c.Item().PaddingTop(8).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text($"Medicamento: {consulta.Medicamento?.Nombre}").Bold();
                                r.ConstantItem(100).AlignRight().Text($"Cant: {consulta.CantidadUsada}").Bold();
                            });
                        }
                    });

                    // FIRMA Y SELLO
                    col.Item().PaddingTop(60).Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().PaddingHorizontal(20).LineHorizontal(1);
                            c.Item().AlignCenter().Text("Firma Responsable").FontSize(9);
                        });
                        row.ConstantItem(60);
                        row.RelativeItem().Column(c => {
                            if (System.IO.File.Exists(_selloPath)) c.Item().AlignCenter().Height(70).Image(_selloPath);
                            else c.Item().AlignCenter().MinWidth(50).MinHeight(50).Border(1).AlignCenter().AlignMiddle().Text("SELLO");
                        });
                    });
                });

                page.Footer().AlignCenter().Text(t => {
                    t.Span("Escuela Cristiana Oasis - Documento Oficial. ").FontSize(8).FontColor(Colors.Grey.Medium);
                    t.Span("Página "); t.CurrentPageNumber();
                });
            });
        });

        // El tercer parámetro fuerza al navegador a tratarlo como una descarga con ese nombre
        return File(pdf.GeneratePdf(), "application/pdf", $"Atencion_{consulta.Estudiante.NombreCompleto}_{consulta.Id}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> Generar(string tipo, DateTime? fecha, int? estudianteId)
    {
        // 1. REPORTE DIARIO
        if (tipo == "diario_consultas" && fecha.HasValue)
        {
            var consultas = await _context.Consultas.Include(c => c.Estudiante)
                .Where(c => c.FechaHora.Date == fecha.Value.Date).OrderBy(c => c.FechaHora).ToListAsync();

            var archivo = GenerarReporteTabla(consultas, $"ATENCIONES DEL DÍA: {fecha.Value:dd/MM/yyyy}");
            return File(archivo, "application/pdf", $"Reporte_Diario_{fecha.Value:yyyyMMdd}.pdf");
        }

        // 2. HISTORIAL COMPLETO ALUMNO
        if (tipo == "estudiante_historial" && estudianteId.HasValue)
        {
            var estudiante = await _context.Estudiantes.FindAsync(estudianteId);
            var consultas = await _context.Consultas.Include(c => c.Estudiante)
                .Where(c => c.EstudianteId == estudianteId).OrderByDescending(c => c.FechaHora).ToListAsync();

            var archivo = GenerarReporteTabla(consultas, $"HISTORIAL CLÍNICO: {estudiante?.NombreCompleto}");
            return File(archivo, "application/pdf", $"Historial_{estudiante?.NombreCompleto}.pdf");
        }

        // 3. STOCK BAJO
        if (tipo == "stock_bajo")
        {
            var bajos = await _context.Medicamentos.Where(m => m.StockActual <= m.StockMinimo).ToListAsync();
            var pdf = Document.Create(c => c.Page(p => {
                p.Margin(30);
                p.Header().Text("ALERTA DE STOCK CRÍTICO").FontSize(18).Bold().FontColor(Colors.Red.Medium);
                p.Content().PaddingVertical(10).Table(t => {
                    t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.ConstantColumn(80); cd.ConstantColumn(80); });
                    t.Header(h => {
                        h.Cell().Background(Colors.Red.Medium).Padding(2).Text("Insumo").FontColor(Colors.White);
                        h.Cell().Background(Colors.Red.Medium).Padding(2).Text("Actual").FontColor(Colors.White);
                        h.Cell().Background(Colors.Red.Medium).Padding(2).Text("Mín.").FontColor(Colors.White);
                    });
                    foreach (var m in bajos)
                    {
                        t.Cell().BorderBottom(0.5f).Padding(3).Text(m.Nombre);
                        t.Cell().BorderBottom(0.5f).Padding(3).Text(m.StockActual.ToString());
                        t.Cell().BorderBottom(0.5f).Padding(3).Text(m.StockMinimo.ToString());
                    }
                });
            })).GeneratePdf();
            return File(pdf, "application/pdf", "Alerta_Stock_Bajo.pdf");
        }

        // 4. MOVIMIENTOS DE FARMACIA
        if (tipo == "movimientos_dia" || tipo == "movimientos_mes")
        {
            var inicio = tipo == "movimientos_dia" ? DateTime.Today : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var movs = await _context.Consultas.Include(c => c.Estudiante).Include(c => c.Medicamento)
                .Where(c => c.FechaHora >= inicio && c.MedicamentoId != null).OrderByDescending(c => c.FechaHora).ToListAsync();

            var pdf = Document.Create(c => c.Page(p => {
                p.Margin(30);
                p.Header().Text("SALIDAS DE MEDICAMENTOS").FontSize(16).Bold().FontColor(ColorEscuela);
                p.Content().Table(t => {
                    t.ColumnsDefinition(cd => { cd.ConstantColumn(60); cd.RelativeColumn(); cd.RelativeColumn(); cd.ConstantColumn(40); });
                    t.Header(h => {
                        h.Cell().Background(ColorEscuela).Text("Fecha").FontColor(Colors.White);
                        h.Cell().Background(ColorEscuela).Text("Alumno").FontColor(Colors.White);
                        h.Cell().Background(ColorEscuela).Text("Medicamento").FontColor(Colors.White);
                        h.Cell().Background(ColorEscuela).Text("Cant.").FontColor(Colors.White);
                    });
                    foreach (var m in movs)
                    {
                        t.Cell().Text(m.FechaHora.ToString("dd/MM/yy"));
                        t.Cell().Text(m.Estudiante?.NombreCompleto);
                        t.Cell().Text(m.Medicamento?.Nombre);
                        t.Cell().Text(m.CantidadUsada.ToString());
                    }
                });
            })).GeneratePdf();
            return File(pdf, "application/pdf", $"Movimientos_Farmacia_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // 5. TENDENCIAS
        if (tipo == "mensual_motivos" && fecha.HasValue)
        {
            var tendencias = await _context.Consultas
                .Where(c => c.FechaHora.Month == fecha.Value.Month && c.FechaHora.Year == fecha.Value.Year)
                .GroupBy(c => c.Motivo).Select(g => new { Motivo = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total).ToListAsync();

            var pdf = Document.Create(c => c.Page(p => {
                p.Margin(40);
                p.Header().Text($"TENDENCIAS - {fecha.Value:MMMM yyyy}").FontSize(16).Bold().FontColor(ColorEscuela);
                p.Content().PaddingVertical(10).Table(t => {
                    t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.ConstantColumn(80); });
                    t.Header(h => {
                        h.Cell().Background(ColorEscuela).Padding(5).Text("Motivo").FontColor(Colors.White);
                        h.Cell().Background(ColorEscuela).Padding(5).Text("N° Casos").FontColor(Colors.White);
                    });
                    foreach (var item in tendencias)
                    {
                        t.Cell().BorderBottom(0.5f).Padding(5).Text(item.Motivo);
                        t.Cell().BorderBottom(0.5f).Padding(5).Text(item.Total.ToString());
                    }
                });
            })).GeneratePdf();
            return File(pdf, "application/pdf", $"Tendencias_{fecha.Value:MMMM_yyyy}.pdf");
        }

        return RedirectToAction("Index");
    }

    private byte[] GenerarReporteTabla(List<Consulta> consultas, string titulo)
    {
        return Document.Create(container => {
            container.Page(page => {
                page.Margin(30);
                page.Header().Text(titulo).FontSize(16).SemiBold().FontColor(ColorEscuela);
                page.Content().PaddingVertical(10).Table(table => {
                    table.ColumnsDefinition(c => { c.ConstantColumn(60); c.RelativeColumn(); c.RelativeColumn(); });
                    table.Header(h => {
                        h.Cell().Background(ColorEscuela).Padding(2).Text("Fecha").FontColor(Colors.White);
                        h.Cell().Background(ColorEscuela).Padding(2).Text("Estudiante").FontColor(Colors.White);
                        h.Cell().Background(ColorEscuela).Padding(2).Text("Diagnóstico").FontColor(Colors.White);
                    });
                    foreach (var c in consultas)
                    {
                        table.Cell().BorderBottom(0.5f).Padding(3).Text(c.FechaHora.ToString("dd/MM/yy")).FontSize(9);
                        table.Cell().BorderBottom(0.5f).Padding(3).Text(c.Estudiante?.NombreCompleto).FontSize(9);
                        table.Cell().BorderBottom(0.5f).Padding(3).Text(c.Diagnostico).FontSize(9);
                    }
                });
                page.Footer().AlignCenter().Text(t => {
                    t.Span("Página "); t.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }
}